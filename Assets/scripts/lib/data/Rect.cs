﻿namespace lib
{
    public class Rect
    {
        public Float X = new Float();
        public Float Y = new Float();
        public Float Width = new Float();
        public Float Height = new Float();

        public Rect(float x = 0,float y = 0,float width = 0,float height = 0)
        {
            X.value = x;
            Y.value = y;
            Width.value = width;
            Height.value = height;
        }
    }
}