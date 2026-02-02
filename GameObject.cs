using Zirconium.OBJ;
using Zirconium.Types.Enums;
using Zirconium.Types.BluePrints;
using OpenTK.Mathematics;
using CubeGPU = Zirconium.Main.Engine.CubeGPU;
using SphereGPU = Zirconium.Main.Engine.SphereGPU;
using TriangleGPU = Zirconium.Main.Engine.TriangleGPU;

namespace Zirconium.Engine
{
    public class GameObject
    {
        public GameObjectType _type;
        public GameObject? parent;
        public int? id;
        public string Name = "GameObject";
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public List<IGpuType> Send = [];

        public GameObject(GameObject? parent, GameObjectType type, string name, string? path, Vector3 pos, Vector3 rot, Vector3 size, Vector3 albedo, float smoothness, Vector3 emission, float emissionStrength, float alpha, float ior = 1.0f, float absorb = 0.0f)
        {
            this.parent = parent;
            _type = type;
            Name = name;
            Position = pos;
            Rotation = rot;
            Scale = size;
            switch (_type)
            {
                default:
                    break;
                case GameObjectType.Cube:
                    Send.Add(new CubeGPU(pos, size, albedo, smoothness, emission, emissionStrength, alpha, ior, absorb));
                    break;
                case GameObjectType.Sphere:
                    Send.Add(new SphereGPU(pos, size.X, albedo, smoothness, emission, emissionStrength, alpha, ior, absorb));
                    break;
                case GameObjectType.Mesh:
                    List<TriangleGPU> tris = [];
                    if (path != null)
                    {
                        ObjLoader.Load(out tris, path, pos, rot, size, albedo, smoothness, emission, emissionStrength, alpha);
                    }
                    foreach (TriangleGPU tri in tris)
                    {
                        Send.Add(tri);
                    }
                    break;
            }
        }

        public void Update(Vector3 newPos, Vector3 newRot, Vector3 newScale)
        {
            if (_type != GameObjectType.Mesh) return;
            Vector3 deltaRot = newRot - Rotation;
            Matrix4 rotMatrix = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(deltaRot.X)) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(deltaRot.Y)) * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(deltaRot.Z));
            foreach (IGpuType gpu in Send)
            {
                TriangleGPU tri = (TriangleGPU)gpu;
                tri.V0 -= new Vector4(Position, 0);
                tri.V0 /= new Vector4(Scale, 1);
                tri.V0 *= rotMatrix;
                tri.V0 *= new Vector4(newScale, 1);
                tri.V1 -= new Vector4(Position, 0);
                tri.V1 /= new Vector4(Scale, 1);
                tri.V1 *= rotMatrix;
                tri.V1 *= new Vector4(newScale, 1);
                tri.V2 -= new Vector4(Position, 0);
                tri.V2 /= new Vector4(Scale, 1);
                tri.V2 *= rotMatrix;
                tri.V2 *= new Vector4(newScale, 1);
                tri.V0 += new Vector4(newPos, 0);
                tri.V1 += new Vector4(newPos, 0);
                tri.V2 += new Vector4(newPos, 0);
            }
            Position = newPos;
            Rotation = newRot;
            Scale = newScale;
        }
    }
}