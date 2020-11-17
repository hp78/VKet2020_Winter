// Created by SHAJIKUworks

using System;
using System.Reflection;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VketBeta
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public sealed class ButtonAttribute : PropertyAttribute
    {
        public Rect FreePosition = new Rect(0f, 0f, 1f, 16f);
        public string Function = null;
        public string Name = null;
        public object[] Parameters = null;

        public ButtonAttribute(string function, string name, params object[] parameters)
        {
            SetParameters(null, null, function, name, parameters);
        }

        public ButtonAttribute(int height, string function, string name, params object[] parameters)
        {
            SetParameters(null, height, function, name, parameters);
        }

        public ButtonAttribute(float x, float width, int y, int height, string function, string name, params object[] parameters)
        {
            SetParameters(new Rect(x, y, width, height), null, function, name, parameters);
        }

        void SetParameters(Rect? freePosition, int? height, string function, string name, params object[] parameters)
        {
            if (freePosition.HasValue)
                FreePosition = freePosition.Value;
            else if (height.HasValue)
                FreePosition.height = height.Value;

            if (function != null)
                Function = function;

            if (name != null)
                Name = name;

            Parameters = parameters;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public sealed class ButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var buttonAttribute = attribute as ButtonAttribute;

            // フリーポジション
            {
                var free = buttonAttribute.FreePosition;
                position.x += position.width * free.x;
                position.width *= free.width;
                position.y += free.y;
                position.height = free.height;
            }

            if (GUI.Button(position, buttonAttribute.Name))
            {
                var objectReferenceValue = property.serializedObject.targetObject;
                var type = objectReferenceValue.GetType();
                var bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var method = type.GetMethod(buttonAttribute.Function, bindingAttr);

                try
                {
                    method.Invoke(objectReferenceValue, buttonAttribute.Parameters);
                }
                catch (AmbiguousMatchException)
                {
                    var format = @"{0}.{1} 関数がオーバーロードされているため関数を特定できません。{0}.{1} 関数のオーバーロードを削除してください";
                    var message = string.Format(format, type.Name, buttonAttribute.Function);
                    Debug.LogError(message, objectReferenceValue);
                }
                catch (ArgumentException)
                {
                    var parameters = string.Join(", ", buttonAttribute.Parameters.Select(c => c.ToString()).ToArray());
                    var format = @"{0}.{1} 関数に引数 {2} を渡すことができません。{0}.{1} 関数の引数の型が正しいかどうかを確認してください";
                    var message = string.Format(format, type.Name, buttonAttribute.Function, parameters);
                    Debug.LogError(message, objectReferenceValue);
                }
                catch (NullReferenceException)
                {
                    var format = @"{0}.{1} 関数は定義されていません。{0}.{1} 関数が定義されているかどうかを確認してください";
                    var message = string.Format(format, type.Name, buttonAttribute.Function);
                    Debug.LogError(message, objectReferenceValue);
                }
                catch (TargetParameterCountException)
                {
                    var parameters = string.Join(", ", buttonAttribute.Parameters.Select(c => c.ToString()).ToArray());
                    var format = @"{0}.{1} 関数に引数 {2} を渡すことができません。{0}.{1} 関数の引数の数が正しいかどうかを確認してください";
                    var message = string.Format(format, type.Name, buttonAttribute.Function, parameters);
                    Debug.LogError(message, objectReferenceValue);
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attri = attribute as ButtonAttribute;
            return Mathf.Max(0, attri.FreePosition.height + attri.FreePosition.y);
        }
    }
#endif
}
