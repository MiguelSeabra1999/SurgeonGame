using System;
using System.Collections.Generic;
using UnityEngine;


namespace Libraries
{
    [Flags]
    public enum Directions : byte
    {
        None = 0,
        Left = 1<<0,
        Right = 1<<1,
        Up = 1<<2,
        Down = 1<<3,
        LeftUp = 1<<4,
        LeftDown = 1<<5,
        RightUp  = 1<<6,
        RightDown = 1<<7
    }
    [Flags]
    public enum BasicDirections : byte
    {
        Left = 1<<0,
        Right = 1<<1,
        Up = 1<<2,
        Down = 1<<3,
    }
    
    [Flags]
    public enum Axis 
    {
        Horizontal = 0,
        Vertical = 1
    }
    public static class GridFunctions
    {
        public static Vector2Int GetCoordNeighbour(Vector2Int coords, Directions direction)
        {
            return coords + directionToVec[direction];
        }        
        public static Directions[] GetComplexDirectionsFromBasic(BasicDirections direction)
        {
            return basicDirectionToComplex[direction];
        }
        public static Directions[] GetDirectionsFromAxis(Axis axis)
        {
            return axisToDirection[axis];
        }

        public static string AxisToString(Axis cutAxis)
        {
            return axisToString[cutAxis];
        }
        
        private static Dictionary<Directions, Vector2Int> directionToVec = new Dictionary<Directions, Vector2Int>()
        {
            {Directions.None, Vector2Int.zero},
            {Directions.Left, Vector2Int.left},
            {Directions.Right, Vector2Int.right},
            {Directions.Up, Vector2Int.up},
            {Directions.Down, Vector2Int.down},
            {Directions.LeftUp, Vector2Int.left + Vector2Int.up},
            {Directions.LeftDown, Vector2Int.left + Vector2Int.down},
            {Directions.RightUp, Vector2Int.right + Vector2Int.up},
            {Directions.RightDown, Vector2Int.right + Vector2Int.down}
        };  
        
        private static Dictionary<BasicDirections, Directions[]> basicDirectionToComplex = new Dictionary<BasicDirections,  Directions[]>()
        {
            {BasicDirections.Left, new Directions[]{Directions.Left, Directions.LeftUp, Directions.LeftDown}},
            {BasicDirections.Right, new Directions[]{Directions.Right, Directions.RightUp, Directions.RightDown}},
            {BasicDirections.Up, new Directions[]{Directions.Up, Directions.LeftUp, Directions.RightUp}},
            {BasicDirections.Down, new Directions[]{Directions.Down, Directions.LeftDown, Directions.RightDown}},
    
        }; 
        
        private static Dictionary<Axis, Directions[]> axisToDirection = new Dictionary<Axis, Directions[]>()
        {
            {Axis.Horizontal, new Directions[]{Directions.Right, Directions.Left}},
            {Axis.Vertical, new Directions[]{Directions.Up, Directions.Down}}
        };

        private static Dictionary<Axis, string> axisToString = new Dictionary<Axis, string>()
        {
            {Axis.Horizontal,"Horizontal"},
            {Axis.Vertical,"Vertical"},
        };

        public static Axis GetOtherAxis(Axis axis)
        {
            if (axis == Axis.Horizontal)
                return Axis.Vertical;
            return Axis.Horizontal;
        }
    }
}