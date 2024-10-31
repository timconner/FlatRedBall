using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace FlatRedBall.Math.Geometry
{
    /// <summary>
    /// A 3-dimensional point using doubles as its components.
    /// </summary>
    public struct Point3D
    {
        /// <summary>
        /// A point at the origin (0,0,0).
        /// </summary>
        public static readonly Point3D Zero = new Point3D(0, 0, 0);

        /// <summary>
        /// The point's X coordinate.
        /// </summary>
        public double X;

        /// <summary>
        /// The point's Y coordinate.
        /// </summary>
        public double Y;

        /// <summary>
        /// The point's Z coordinate.
        /// </summary>
        public double Z;

        #region Properties and Overloaded Operators
        public static Point3D operator -(Point3D p1, Vector3 p2)
        {
            return new Point3D(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }


        public static Point3D operator +(Point3D p1, Vector3 v2)
        {
            return new Point3D(p1.X + v2.X, p1.Y + v2.Y);
        }

        #endregion

        #region Methods

        #region Constructors

        public Point3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Point3D(Vector2 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Z = 0;
        }

        public Point3D(Vector3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
        }

        public Point3D(double x, double y)
        {
            X = x;
            Y = y;
            Z = 0;
        }        
        
        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #endregion

        public double Length()
        {
            return System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public Vector2 ToVector2()
        {
            return new Vector2((float)X, (float)Y);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        #endregion
    }
}
