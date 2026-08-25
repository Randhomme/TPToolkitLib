using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPToolkitLib.MeshScene.Classes
{
    public class MsbKeyframe
    {
        public float Time { get; set; }
        public float Value { get; set; }
        public int Smoothing { get; set; }
        public float Tension { get; set; }
        public float Continuity { get; set; }
        public float Bias { get; set; }
        public float IncomingTangent { get; set; }
        public float OutgoingTangent { get; set; }
    }
}
