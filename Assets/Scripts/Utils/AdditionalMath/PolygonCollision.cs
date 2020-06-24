using UnityEngine;

namespace Utils.AdditionalMath {
    public static class PolygonCollision {
        // Check if polygon A is going to collide with polygon B for the given velocity
    public static bool DoesCollide(this Polygon polygonA, Polygon polygonB) {
        var edgeCountA = polygonA.Edges().Count;
        var edgeCountB = polygonB.Edges().Count;

        // Loop through all the edges of both polygons
        for (var edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++) {
            var edge = edgeIndex < edgeCountA ? polygonA.Edges()[edgeIndex] : polygonB.Edges()[edgeIndex - edgeCountA];

            // Find the axis perpendicular to the current edge
            var axis = new Vector2(-edge.y, edge.x);
            axis.Normalize();

            // Find the projection of the polygon on the current axis
            float minA = 0;
            float minB = 0;
            float maxA = 0;
            float maxB = 0;
            ProjectPolygon(axis, polygonA, ref minA, ref maxA);
            ProjectPolygon(axis, polygonB, ref minB, ref maxB);

            // Check if the polygon projections are currentlty intersecting
            if (IntervalDistance(minA, maxA, minB, maxB) > 0) return false;
        }

        return true;
    }

    // Calculate the distance between [minA, maxA] and [minB, maxB]
    // The distance will be negative if the intervals overlap
    private static float IntervalDistance(float minA, float maxA, float minB, float maxB) {
        if (minA < minB) {
            return minB - maxA;
        }
        return minA - maxB;
    }

    // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
    private static void ProjectPolygon(Vector2 axis, Polygon polygon, ref float min, ref float max) {
        // To project a point on an axis use the dot product
        var d = Vector2.Dot(axis, polygon.Points()[0]);
        min = d;
        max = d;
        for (var i = 0; i < polygon.Points().Count; i++) {
            d = Vector2.Dot(polygon.Points()[i], axis);
            if (d < min) {
                min = d;
            } else {
                if (d > max) {
                    max = d;
                }
            }
        }
    }
    }
}