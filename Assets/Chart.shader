Shader "Unlit/Chart"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}


        _Repetitions("Repetitions", Range(3, 12)) = 5
        _RadialLineThickness("RadialLinesThickness", Range(0.001, 0.05)) = 0.004
        _SectionThickness("SectionThickness", Range(0.1, 0.5)) = 0.2
        _StatCircles("StatCircles", Range(0, 0.02)) = 0.0125
        _StatLinesThickness("StatLinesThickness", Range(0, 0.02)) = 0.003

        _Bg("BG", Color) = (.8, .5, .2, 0.)
        [HDR] _CirclesColor("CirclesColor", Color) = (.8, 0.21, 0.53)
        [HDR] _ChartStatLines("StatLinesColor", Color) = (.87, .32, 0.42)
        _CoveredAreaColor("CoveredAreaColor", Color) = (1, 0.75, 0.75)
        _ChartBaseColor("ChartBaseColor", Color) = (.8, .8, .8)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        ColorMask RGB

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"
            #include "Packages/com.quizandpuzzle.shaderlib/Runtime/math.cginc"
            #include "Packages/com.quizandpuzzle.shaderlib/Runtime/sdf.cginc"


            float _Repetitions;
            float _SectionThickness;
            float _RadialLineThickness;
            float _StatLinesThickness;
            float _StatCircles;
            float _Stats[6];

            float4 _Bg;

            float3 _CirclesColor;
            float3 _CoveredAreaColor;
            float3 _ChartBaseColor;
            float3 _ChartStatLines;


            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            Interpolators vert(MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float opSmoothUnion(float d1, float d2, float k)
            {
                float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
                return lerp(d2, d1, h) - k * h * (1.0 - h);
            }

            float opSmoothSubtraction(float d1, float d2, float k)
            {
                float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
                return lerp(d2, -d1, h) + k * h * (1.0 - h);
            }

            float opSmoothIntersection(float d1, float d2, float k)
            {
                float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
                return lerp(d2, d1, h) + k * h * (1.0 - h);
            }

            float ChartSDF(float2 uv, float2 polygon[11], float count)
            {
                return PolygonSDF(uv, polygon, count);
            }

            float Hexagon(float2 uv)
            {
                uv *= PI;
                float radial = atan2(uv.x, uv.y) + PI;
                float sides = 6;
                float side = UNITY_TWO_PI / sides;
                return cos(floor(radial / side + 0.5) * side - radial) * length(uv);
            }
            
            float Remap(float oa, float ob, float na, float nb, float val)
            {
                return (val - oa) / (ob - oa) * (nb - na) + na;
            }


            float ChartSections(float2 uv, float domainRange, float repetitions, float thickness)
            {
                float hexagon = Hexagon(uv * domainRange);
                float sections = frac(hexagon * repetitions);
                float sectionsContinuous = length(Remap(0, 1, -1, 1, sections));
                return sectionsContinuous - 1 + thickness;
            }

            float ChartRadialLines(float2 uv, float2 dir, float thickness)
            {
                float segment = SegmentSDF(uv, float2(0, 0), dir * 2);
                return segment - thickness;
            }

            float Sample(float s, float offset = 1)
            {
                float e = fwidth(s) * offset;
                return smoothstep(e, -e, s);
            }


            float4 frag(Interpolators i) : SV_Target
            {
                float2 uvCentered = i.uv;
                uvCentered -= 0.5;

                float3 color = float3(0, 0, 0);
                float angle = UNITY_TWO_PI / 6;

                float2 polygon[11];
                float maskSize = 1.05;

                float sectionStep = (maskSize / 2) * .74 / _Repetitions;
                float circles = length(uvCentered);
                float chartRadialLines = length(uvCentered);

                for (int i = 0; i < 6; i++)
                {
                    float2 dir = float2(cos(angle * i), sin(angle * i));
                    float2 pos = dir * sectionStep * _Stats[i];

                    polygon[i] = pos;
                    chartRadialLines = min(chartRadialLines, ChartRadialLines(uvCentered, dir, _RadialLineThickness));
                    circles = min(circles, length(uvCentered - pos) - _StatCircles);
                }

                float chartSections = ChartSections(uvCentered, 1 + 1 - maskSize, _Repetitions, _SectionThickness);
                float hexMask = Hexagon(uvCentered) + (1 - maskSize);
                float statsCoveredArea = ChartSDF(uvCentered, polygon, 6);
                float statLines = abs(statsCoveredArea) - _StatLinesThickness;

                hexMask = smoothstep(maskSize, maskSize - 0.01, hexMask);
                chartSections = Sample(chartSections, -1);
                statsCoveredArea = Sample(statsCoveredArea);
                circles = Sample(circles);
                statLines = Sample(statLines);
                chartRadialLines = Sample(chartRadialLines);

                float chartBase = max(chartSections, chartRadialLines);

                color = hexMask * _Bg;
                color = lerp(color, _ChartBaseColor, chartBase);
                color = lerp(color, _CoveredAreaColor, saturate(statsCoveredArea - chartBase));
                color = lerp(color, _ChartStatLines, statLines);
                color = lerp(color, _CirclesColor, circles);

                float alpha = chartBase + statsCoveredArea * 0.75 + statLines + circles;


                return saturate(float4(color, alpha * hexMask + hexMask * _Bg.a));
            }
            ENDCG
        }
    }
}