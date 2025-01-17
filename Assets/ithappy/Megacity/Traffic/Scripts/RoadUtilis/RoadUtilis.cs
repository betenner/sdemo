namespace ITHappy
{
    using ITHappy;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Splines;
    using UnityEngine.UIElements;

    public static class RoadUtilis
    {
        public static void ParseSplinesToCurves(Spline[] splines, List<TrafficManager.SplineData> curves, Transform transform, int crossIndex = -1)
        {
            int curveIterator = 0;
            foreach (var spline in splines)
            {
                for (int i = 0; i < spline.GetCurveCount(); i++, curveIterator++)
                {
                    var curve = spline.GetCurve(i);
                    var length = spline.GetCurveLength(i);
                    var outputs = new MathExtension.Int3(-1, -1, -1);

                    var startUp = transform.TransformDirection(spline.GetCurveUpVector(i, 0f)).normalized;
                    var endUp = transform.TransformDirection(spline.GetCurveUpVector(i, 1f)).normalized;

                    curves.Add(new(outputs, transform.TransformPoint(curve.P0), transform.TransformPoint(curve.P1),
                        transform.TransformPoint(curve.P2), transform.TransformPoint(curve.P3), startUp, endUp, crossIndex, length));
                }
            }
        }

        public static void GetCurvesCount(Spline[] splines, out int count)
        {
            count = 0;
            foreach (var spline in splines)
            {
                count += spline.GetCurveCount();
            }
        }

        public static void ConnectSplines(TrafficManager.SplineData[] curves, int count, float mergeDistance)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (Vector3.SqrMagnitude(curves[i].p3 - curves[j].p0) < mergeDistance)
                    {
                        curves[j].p0 = curves[i].p3;

                        if (curves[i].outputs.x < 0)
                        {
                            curves[i].outputs.x = j;
                        }
                        else
                        {
                            if (curves[i].outputs.y < 0)
                            {
                                curves[i].outputs.y = j;
                            }
                            else if (curves[i].outputs.z < 0)
                            {
                                curves[i].outputs.z = j;
                            }
                        }
                    }
                }
            }
        }
    }
}