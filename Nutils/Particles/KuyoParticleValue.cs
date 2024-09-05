using System;
using System.Reflection;
using RWCustom;
using SlugBase.SaveData;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Nutils.Particles
{

    public static class KuyoCustom
    {
        public static T GetValue<T>(this SlugBaseSaveData saveData, string name) where T : class, new()
        {
            if(saveData.TryGet(name,out T value))
                return value;
            return new T();
        }
        public static Type[] SafeGetTypes(this Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }

        public static float SimpleCurve(float value,params (float min, float max,float toMin,float toMax, float pow)[] param)
        {
            var a = param[param.Length-1];
            for (int i = 0; i < param.Length; i++)
            {
                if (param[i].max > value)
                {
                    a = param[i];
                    break;
                }
            }

            return Custom.LerpMap(value, a.min, a.max, a.toMin, a.toMax, a.pow);
        }

        public static Color RandomRange(Color a, Color b)
        {
            return new Color(Random.Range(a.r, b.r), Random.Range(a.g, b.g), Random.Range(a.b, b.b),
                Random.Range(a.a, b.a));
        }

        public static Vector2 RandomRange(Vector2 a, Vector2 b)
        {
            return new Vector2(Random.Range(a.x, b.x), Random.Range(a.y, b.y));
        }
    }


    public interface IParticleValue<out TValueType>
    {
        TValueType GetValue(float time, int? seed = null);
    }


    #region Float

    public class ConstFloat : IParticleValue<float>
    {
        public static implicit operator ConstFloat(float b) => new(b);


        private float value;
        public ConstFloat(float f) => value = f;
        public float GetValue(float time, int? seed) => value;

    }

    public class UniformFloat : IParticleValue<float>
    {
        private readonly float min;
        private readonly float max;
        public UniformFloat(float min, float max) => (this.min, this.max) = (min, max);

        public float GetValue(float time, int? seed)
        {
            if (seed != null)
            {
                var state = Random.state;
                Random.InitState(seed.Value);
                var re = Random.Range(min, max);
                Random.state = state;
                return re;
            }
            return Random.Range(min, max);
        }

    }

    public class ConstCurveFloat : IParticleValue<float>
    {
        private readonly Func<float, float> curveOutput;
        public ConstCurveFloat(Func<float, float> curveOutput) => this.curveOutput = curveOutput;
        public float GetValue(float time, int? seed)
        {
            return curveOutput(time);
        }
    }

    public class UniformCurveFloat : IParticleValue<float>
    {
        private readonly Func<float, float> curveOutputA;
        private readonly Func<float, float> curveOutputB;

        public UniformCurveFloat(Func<float, float> curveOutputA, Func<float, float> curveOutputB)
            => (this.curveOutputA, this.curveOutputB) = (curveOutputA, curveOutputB);
        public float GetValue(float time, int? seed)
        {
            if (seed != null)
            {
                var state = Random.state;
                Random.InitState(seed.Value);
                var re = Mathf.Lerp(curveOutputA(time), curveOutputB(time), Random.value);
                Random.state = state;
                return re;
            }
            return Mathf.Lerp(curveOutputA(time), curveOutputB(time), Random.value);
        }
    }

    #endregion


    #region Vector2

    public class ConstVector2 : IParticleValue<Vector2>
    {
        public static implicit operator ConstVector2(Vector2 b) => new(b);

        private Vector2 value;
        public ConstVector2(Vector2 f) => value = f;
        public Vector2 GetValue(float time, int? seed) => value;

    }

    public class UniformVector2 : IParticleValue<Vector2>
    {
        private readonly Vector2 min;
        private readonly Vector2 max;
        private readonly bool dirRandom;
        public UniformVector2(Vector2 min, Vector2 max, bool dirRandom = false) => 
            (this.min, this.max, this.dirRandom) = (min, max, dirRandom);

        public Vector2 GetValue(float time, int? seed)
        {
            if (seed != null)
            {
                var state = Random.state;
                Random.InitState(seed.Value);
                Vector2 re = dirRandom
                    ? Vector3.Slerp(min, max, Random.value) * Random.Range(min.magnitude, max.magnitude)
                    : KuyoCustom.RandomRange(min, max);
                Random.state = state;
                return re;
            }
            return dirRandom
                    ? Vector3.Slerp(min, max, Random.value) * Random.Range(min.magnitude, max.magnitude)
                    : KuyoCustom.RandomRange(min, max);
        }

    }

    public class ConstCurveVector2 : IParticleValue<Vector2>
    {
        private readonly Func<float, Vector2> curveOutput;
        public ConstCurveVector2(Func<float, Vector2> curveOutput) => this.curveOutput = curveOutput;
        public Vector2 GetValue(float time, int? seed)
        {
            return curveOutput(time);
        }
    }

    public class UniformCurveVector2 : IParticleValue<Vector2>
    {
        private readonly Func<float, Vector2> curveOutputA;
        private readonly Func<float, Vector2> curveOutputB;

        public UniformCurveVector2(Func<float, Vector2> curveOutputA, Func<float, Vector2> curveOutputB)
            => (this.curveOutputA, this.curveOutputB) = (curveOutputA, curveOutputB);
        public Vector2 GetValue(float time, int? seed)
        {
            if (seed != null)
            {
                var state = Random.state;
                Random.InitState(seed.Value);
                var re = Vector2.Lerp(curveOutputA(time), curveOutputB(time), Random.value);
                Random.state = state;
                return re;
            }
            return Vector2.Lerp(curveOutputA(time), curveOutputB(time), Random.value);
        }
    }

    #endregion


    #region Color

    public class ConstColor : IParticleValue<Color>
    {
        public static implicit operator ConstColor(Color b) => new(b);

        private readonly Color value;
        public ConstColor(Color f) => value = f;
        public Color GetValue(float time, int? seed) => value;

    }

    public class UniformColor : IParticleValue<Color>
    {
        private readonly Color min;
        private readonly Color max;
        private readonly bool useLerp;
        public UniformColor(Color min, Color max,bool useLerp = false) => (this.min, this.max,  this.useLerp) = (min, max, useLerp);

        public Color GetValue(float time, int? seed)
        {
            if (seed != null)
            {
                var state = Random.state;
                Random.InitState(seed.Value);
                Color re = useLerp ? Color.Lerp(min, max, Random.value) : KuyoCustom.RandomRange(min, max);
                Random.state = state;
                return re;
            }
            
            return useLerp ? Color.Lerp(min,max,Random.value) : KuyoCustom.RandomRange(min, max);
        }

    }

    public class ConstCurveColor : IParticleValue<Color>
    {
        private readonly Func<float, Color> curveOutput;
        public ConstCurveColor(Func<float, Color> curveOutput) => this.curveOutput = curveOutput;
        public Color GetValue(float time, int? seed)
        {
            return curveOutput(time);
        }
    }

    public class UniformCurveColor : IParticleValue<Color>
    {
        private readonly Func<float, Color> curveOutputA;
        private readonly Func<float, Color> curveOutputB;


        public UniformCurveColor(Func<float, Color> curveOutputA, Func<float, Color> curveOutputB)
            => (this.curveOutputA, this.curveOutputB) = (curveOutputA, curveOutputB);
        public Color GetValue(float time, int? seed)
        {
            if (seed != null)
            {
                var state = Random.state;
                Random.InitState(seed.Value);
                var re = Color.Lerp(curveOutputA(time), curveOutputB(time), Random.value);
                Random.state = state;
                return re;
            }
            return Color.Lerp(curveOutputA(time), curveOutputB(time), Random.value);
        }
    }

    #endregion
}
