using System.Collections.Generic;
using UnityEngine;

namespace Habrador_Computational_Geometry
{
    public class Box
    {
        //The corners
        public MyVector3 topFR;
        public MyVector3 topFL;
        public MyVector3 topBR;
        public MyVector3 topBL;

        public MyVector3 bottomFR;
        public MyVector3 bottomFL;
        public MyVector3 bottomBR;
        public MyVector3 bottomBL;


        //Generate a bounding box from a mesh in world space
        //Is similar to AABB but takes orientation into account so is sometimes smaller
        //which is useful for collision detection
        public Box(Mesh mesh, Transform meshTrans)
        {
            Bounds bounds = mesh.bounds;

            Vector3 halfSize = bounds.extents;

            Vector3 xVec = Vector3.right * halfSize.x;
            Vector3 yVec = Vector3.up * halfSize.y;
            Vector3 zVec = Vector3.forward * halfSize.z;

            Vector3 top = bounds.center + yVec;
            Vector3 bottom = bounds.center - yVec;

            Vector3 topFR = top + zVec + xVec;
            Vector3 topFL = top + zVec - xVec;
            Vector3 topBR = top - zVec + xVec;
            Vector3 topBL = top - zVec - xVec;

            Vector3 bottomFR = bottom + zVec + xVec;
            Vector3 bottomFL = bottom + zVec - xVec;
            Vector3 bottomBR = bottom - zVec + xVec;
            Vector3 bottomBL = bottom - zVec - xVec;


            //Local to world space
            topFR = meshTrans.TransformPoint(topFR);
            topFL = meshTrans.TransformPoint(topFL);
            topBR = meshTrans.TransformPoint(topBR);
            topBL = meshTrans.TransformPoint(topBL);

            bottomFR = meshTrans.TransformPoint(bottomFR);
            bottomFL = meshTrans.TransformPoint(bottomFL);
            bottomBR = meshTrans.TransformPoint(bottomBR);
            bottomBL = meshTrans.TransformPoint(bottomBL);

            this.topFR = topFR.ToMyVector3();
            this.topFL = topFL.ToMyVector3();
            this.topBR = topBR.ToMyVector3();
            this.topBL = topBL.ToMyVector3();

            this.bottomFR = bottomFR.ToMyVector3();
            this.bottomFL = bottomFL.ToMyVector3();
            this.bottomBR = bottomBR.ToMyVector3();
            this.bottomBL = bottomBL.ToMyVector3();
        }



        //Its common that we want to display this box for debugging, so return a list with edges that form the box
        public List<Edge3> GetEdges()
        {
            List<Edge3> edges = new List<Edge3>()
            {
                new(topFR, topFL),
                new(topFL, topBL),
                new(topBL, topBR),
                new(topBR, topFR),

                new(bottomFR, bottomFL),
                new(bottomFL, bottomBL),
                new(bottomBL, bottomBR),
                new(bottomBR, bottomFR),

                new(topFR, bottomFR),
                new(topFL, bottomFL),
                new(topBL, bottomBL),
                new(topBR, bottomBR),
            };

            return edges;
        }



        //Get all corners of the box
        public HashSet<MyVector3> GetCorners()
        {
            HashSet<MyVector3> corners = new HashSet<MyVector3>()
            {
                topFR,
                topFL,
                topBR,
                topBL,

                bottomFR,
                bottomFL,
                bottomBR,
                bottomBL,
            };

            return corners;
        }
    }
}
