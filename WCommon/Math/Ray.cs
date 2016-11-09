using OpenTK;
using System;

namespace WindEditor
{
    public struct FRay
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public FRay(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        /// <summary>
        /// Returns a point along the ray.
        /// </summary>
        /// <param name="distance">The distance along the ray in which to return the point of.</param>
        /// <returns>Worldspace position of the point along the ray.</returns>
        public Vector3 GetPoint(float distance)
        {
            return Origin + (Direction * distance);
        }

        /// <summary>
        /// Determines if the ray intersects with the given plane.
        /// </summary>
        /// <param name="plane">Plane to check intersection with</param>
        /// <returns></returns>
        public bool IntersectsWith(FPlane plane, out float intersectDist)
        {
            float a = Vector3.Dot(Direction, plane.Normal);
            float num = -Vector3.Dot(Origin, plane.Normal) - plane.Distance;

            if (Math.Abs(a) < float.Epsilon)
            {
                intersectDist = 0f;
                return false;
            }
            intersectDist = num / a;
            return intersectDist > 0f;
        }

        /// <summary>
        /// Determines if the ray intersects with the given bounding box.
        /// </summary>
        /// <param name="boundingBox">Bounding box to check intersection with</param>
        /// <returns></returns>
        public bool IntersectsWith(FAABox boundingBox, out float intersectionDist)
        {
            Vector3 dirFrac = new Vector3(1.0f / Direction.X, 1.0f / Direction.Y, 1.0f / Direction.Z);

            float t1 = (boundingBox.Min.X - Origin.X) * dirFrac.X;
            float t2 = (boundingBox.Max.X - Origin.X) * dirFrac.X;
            float t3 = (boundingBox.Min.Y - Origin.Y) * dirFrac.Y;
            float t4 = (boundingBox.Max.Y - Origin.Y) * dirFrac.Y;
            float t5 = (boundingBox.Min.Z - Origin.Z) * dirFrac.Z;
            float t6 = (boundingBox.Max.Z - Origin.Z) * dirFrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            if (tmax < 0)
            {
                intersectionDist = tmax;
                return false;
            }

            if (tmin > tmax)
            {
                intersectionDist = tmax;
                return false;
            }

            else
            {
                intersectionDist = tmin;
                return true;
            }
        }

        /// <summary>
        /// Determines if the ray intersects with the given sphere.
        /// </summary>
        /// <param name="sphere">Sphere to check intersection with</param>
        /// <returns></returns>
        public bool IntersectsWith(FSphere sphere, out float intersectionDist)
        {
            // From http://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code

            // This is the ray origin's offset from the center of the sphere.
            Vector3 offsetFromSphereCenter = Origin - sphere.Center;

            float radiusSquared = sphere.Radius * sphere.Radius;
            float offsetDotRadiusSquared = Vector3.Dot(offsetFromSphereCenter, Direction);

            // If either of these are true, that means the sphere is behind or surrounding the starting point.
            // Basically, the sphere is behind the ray's origin, or the ray's origin is within the sphere
            if (offsetDotRadiusSquared < 0 || Vector3.Dot(offsetFromSphereCenter, offsetFromSphereCenter) < radiusSquared)
            {
                intersectionDist = float.MaxValue;
                return false;
            }

            // Flatten the ray offset onto the plan passing through the sphere's center perpendicular to the ray.
            // This gives the closest approach of the ray to the center of the sphere.
            Vector3 flattening = offsetFromSphereCenter - offsetDotRadiusSquared * Direction;

            float flatteningSquared = Vector3.Dot(flattening, flattening);

            // If this is true, the closest approach of the ray to the sphere is outside the sphere.
            // Basically, the ray doesn't intersect the sphere.
            if (flatteningSquared > radiusSquared)
            {
                intersectionDist = float.MaxValue;
                return false;
            }

            // This is the distance from the plane where the ray enters and exits the sphere.
            float distanceFromPlane = (float)Math.Sqrt(radiusSquared - flatteningSquared);

            // This is the intersection point relative to the sphere center.
            Vector3 intersection = flattening - distanceFromPlane * Direction;

            Vector3 dirToIntersection = sphere.Center + intersection;
            dirToIntersection.Normalize();

            intersectionDist = (Origin - intersection).Length;
            return true;
        }

        public override string ToString()
        {
            return string.Format("Origin: {0:F3}, {1:F3}, {2:F3}, Direction: {3:F3}, {4:F3}, {5:F3}", Origin.X, Origin.Y, Origin.Z, Direction.X, Direction.Y, Direction.Z);
        }
    }
}
