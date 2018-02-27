using UnityEngine;

namespace MathUtils
{
    public class FruitMath : MonoBehaviour {

        /// <summary>
        /// Get the angle betweenthe <paramref name="vector" and x axis/>. 
        /// The angle is always positive between 0 and 360 and is measured counterclockwise.
        /// </summary>
        public static float GetAngleFromXAxis(Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x);
            if (angle < 0)
            {
                angle += 2 * Mathf.PI;
            }
            return 360 * angle / (2 * Mathf.PI);
        }

        public static float GetAngleBetweenTwoPoints(Vector2 point1, Vector2 point2)
        {
            float result = (GetAngleFromXAxis(point2) - GetAngleFromXAxis(point1));
            if (point1.x > 0 && point2.x > 0)
            {
                if (point1.y > 0 && point2.y < 0) result -= 360;
                else if (point1.y < 0 && point2.y > 0) result += 360;
            }

            return result;
        }

        //Get the intersection between a line and a plane. 
        //If the line and plane are not parallel, the function outputs true, otherwise false.
        public static bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {

            float length;
            float dotNumerator;
            float dotDenominator;
            Vector3 vector;
            intersection = Vector3.zero;

            //calculate the distance between the linePoint and the line-plane intersection point
            dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            dotDenominator = Vector3.Dot(lineVec, planeNormal);

            //line and plane are not parallel
            if (dotDenominator != 0.0f)
            {
                length = dotNumerator / dotDenominator;

                //create a vector from the linePoint to the intersection point
                vector = FruitMath.SetVectorLength(lineVec, length);

                //get the coordinates of the line-plane intersection point
                intersection = linePoint + vector;

                return true;
            }

            //output not valid
            else
            {
                return false;
            }
        }

        //create a vector of direction "vector" with length "size"
        public static Vector3 SetVectorLength(Vector3 vector, float size)
        {

            //normalize the vector
            Vector3 vectorNormalized = Vector3.Normalize(vector);

            //scale the vector
            return vectorNormalized *= size;
        }
    }
}
