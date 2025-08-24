using System;
using UnityEngine;
using UnityEditor;

namespace World.Blocks.Atlases
{
    [Serializable]
    public struct BlockAtlasCategory : IEquatable<BlockAtlasCategory>
    {
        [SerializeField] private string name; // Unity сериализует только поля

        public string Name => name;

        private BlockAtlasCategory(string name) => this.name = name;

        // Предопределённые "стандартные" категории
        public static readonly BlockAtlasCategory Empty = new("Empty");

        // Можно создать из строки (для сериализации/конфига)
        public static BlockAtlasCategory FromString(string name) => new(name);

        // Для сериализации
        public override string ToString() => Name;

        // Сравнение
        public bool Equals(BlockAtlasCategory other) =>
            string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) =>
            obj is BlockAtlasCategory other && Equals(other);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Name ?? "");

        public static bool operator ==(BlockAtlasCategory left, BlockAtlasCategory right) => left.Equals(right);
        public static bool operator !=(BlockAtlasCategory left, BlockAtlasCategory right) => !left.Equals(right);

        // Неявные приведения
        public static implicit operator string(BlockAtlasCategory category) => category.Name;
        public static implicit operator BlockAtlasCategory(string name) => new(name);
    }

    [CustomPropertyDrawer(typeof(BlockAtlasCategory))]
    public class BlockAtlasCategoryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Достаём приватное поле "name" из твоего struct
            SerializedProperty nameProp = property.FindPropertyRelative("name");

            EditorGUI.BeginProperty(position, label, property);
            nameProp.stringValue = EditorGUI.TextField(position, label, nameProp.stringValue);
            EditorGUI.EndProperty();
        }
    }
}