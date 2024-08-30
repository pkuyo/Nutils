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
        TValueType GetValue(bool rand, float time);
    }


    #region Float

    public class ConstFloat : IParticleValue<float>
    {
        public static implicit operator ConstFloat(float b) => new(b);


        private float value;
        public ConstFloat(float f) => value = f;
        public float GetValue(bool rand, float time) => value;

    }

    public class UniformFloat : IParticleValue<float>
    {
        private readonly float min;
        private readonly float max;
        private float randValue;
        public UniformFloat(float min, float max) => (this.min, this.max, randValue) = (min, max, Mathf.Lerp(min, max, 0.5f));

        public float GetValue(bool rand, float time)
        {
            if (rand)
                return randValue = Random.Range(min, max);
            return randValue;
        }

    }

    public class ConstCurveFloat : IParticleValue<float>
    {
        private readonly Func<float, float> curveOutput;
        public ConstCurveFloat(Func<float, float> curveOutput) => this.curveOutput = curveOutput;
        public float GetValue(bool rand, float time)
        {
            return curveOutput(time);
        }
    }

    public class UniformCurveFloat : IParticleValue<float>
    {
        private readonly Func<float, float> curveOutputA;
        private readonly Func<float, float> curveOutputB;
        private float randValue = 0.5f;

        public UniformCurveFloat(Func<float, float> curveOutputA, Func<float, float> curveOutputB)
            => (this.curveOutputA, this.curveOutputB) = (curveOutputA, curveOutputB);
        public float GetValue(bool rand, float time)
        {
            if (rand)
                randValue = Random.value;
            return Mathf.Lerp(curveOutputA(time), curveOutputB(time), randValue);
        }
    }

    #endregion


    #region Vector2

    public class ConstVector2 : IParticleValue<Vector2>
    {
        public static implicit operator ConstVector2(Vector2 b) => new(b);

        private Vector2 value;
        public ConstVector2(Vector2 f) => value = f;
        public Vector2 GetValue(bool rand, float time) => value;

    }

    public class UniformVector2 : IParticleValue<Vector2>
    {
        private readonly Vector2 min;
        private readonly Vector2 max;
        private Vector2 randValue;
        private bool dirRandom;
        public UniformVector2(Vector2 min, Vector2 max, bool dirRandom = false) => 
            (this.min, this.max, randValue, this.dirRandom) = (min, max, Vector2.Lerp(min, max, 0.5f), dirRandom);

        public Vector2 GetValue(bool rand, float time)
        {
            if (rand)
                return randValue = dirRandom
                    ? Vector3.Slerp(min, max, Random.value) * Random.Range(min.magnitude, max.magnitude)
                    : KuyoCustom.RandomRange(min, max);
            return randValue;
        }

    }

    public class ConstCurveVector2 : IParticleValue<Vector2>
    {
        private readonly Func<float, Vector2> curveOutput;
        public ConstCurveVector2(Func<float, Vector2> curveOutput) => this.curveOutput = curveOutput;
        public Vector2 GetValue(bool rand, float time)
        {
            return curveOutput(time);
        }
    }

    public class UniformCurveVector2 : IParticleValue<Vector2>
    {
        private readonly Func<float, Vector2> curveOutputA;
        private readonly Func<float, Vector2> curveOutputB;
        private float randValue = 0.5f;

        public UniformCurveVector2(Func<float, Vector2> curveOutputA, Func<float, Vector2> curveOutputB)
            => (this.curveOutputA, this.curveOutputB) = (curveOutputA, curveOutputB);
        public Vector2 GetValue(bool rand, float time)
        {
            if (rand)
                randValue = Random.value;
            return Vector2.Lerp(curveOutputA(time), curveOutputB(time), randValue);
        }
    }

    #endregion


    #region Color

    public class ConstColor : IParticleValue<Color>
    {
        public static implicit operator ConstColor(Color b) => new(b);

        private Color value;
        public ConstColor(Color f) => value = f;
        public Color GetValue(bool rand, float time) => value;

    }

    public class UniformColor : IParticleValue<Color>
    {
        private readonly Color min;
        private readonly Color max;
        private Color randValue;
        private readonly bool useLerp;
        public UniformColor(Color min, Color max,bool useLerp = false) => (this.min, this.max, randValue, this.useLerp) = (min, max, Color.Lerp(min, max, 0.5f), useLerp);

        public Color GetValue(bool rand, float time)
        {
            if (rand)
                return randValue = useLerp ? Color.Lerp(min,max,Random.value) : KuyoCustom.RandomRange(min, max);
            return randValue;
        }

    }

    public class ConstCurveColor : IParticleValue<Color>
    {
        private readonly Func<float, Color> curveOutput;
        public ConstCurveColor(Func<float, Color> curveOutput) => this.curveOutput = curveOutput;
        public Color GetValue(bool rand, float time)
        {
            return curveOutput(time);
        }
    }

    public class UniformCurveColor : IParticleValue<Color>
    {
        private readonly Func<float, Color> curveOutputA;
        private readonly Func<float, Color> curveOutputB;
        private float randValue = 0.5f;

        public UniformCurveColor(Func<float, Color> curveOutputA, Func<float, Color> curveOutputB)
            => (this.curveOutputA, this.curveOutputB) = (curveOutputA, curveOutputB);
        public Color GetValue(bool rand, float time)
        {
            if (rand)
                randValue = Random.value;
            return Color.Lerp(curveOutputA(time), curveOutputB(time), randValue);
        }
    }

    #endregion
}
