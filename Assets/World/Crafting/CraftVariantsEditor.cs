
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace World.Crafting
{
    // Переопределяем как в инспекторе будет выглядеть "Какие ингредие́нты нужны для крафта предмета"
    [CustomPropertyDrawer(typeof(CraftVariant))]
    public class CraftVariantDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Получаем список ингредиентов
            var ingredientsProp = property.FindPropertyRelative("Ingredients");

            string displayName = "";

            if (ingredientsProp.arraySize > 0)
            {
                for (int i = 0; i < ingredientsProp.arraySize; i++)
                {
                    if (!string.IsNullOrEmpty(displayName)) displayName += " + ";
                    SerializedProperty ingredient = ingredientsProp.GetArrayElementAtIndex(i);
                    displayName += CraftIngredientDrawer.GetDisplayName(ingredient);
                }
            }
            else
            {
                displayName = "Empty Variant";
            }

            // Отрисовываем как обычно, но с новым label
            EditorGUI.PropertyField(position, property, new GUIContent(displayName), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }

    // Переопределяем как в инспекторе будет выглядеть "Ингредие́нт для крафта"
    [CustomPropertyDrawer(typeof(CraftIngredient))]
    public class CraftIngredientDrawer : PropertyDrawer
    {
        public static string GetDisplayName(SerializedProperty ingredient)
        {
            var typeProp = ingredient.FindPropertyRelative("Type");
            var itemTypeProp = ingredient.FindPropertyRelative("ItemType");
            var itemNameProp = ingredient.FindPropertyRelative("ItemName");
            var quantityProp = ingredient.FindPropertyRelative("Quantity");

            CraftIngredientType type = (CraftIngredientType)typeProp.enumValueIndex;

            if (type == CraftIngredientType.TypeItem)
                return $"Type={itemTypeProp.stringValue}({quantityProp.intValue})";

            return $"{itemNameProp.stringValue}({quantityProp.intValue})";
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Вытаскиваем свойства
            var typeProp = property.FindPropertyRelative("Type");
            var itemNameProp = property.FindPropertyRelative("ItemName");
            var itemTypeProp = property.FindPropertyRelative("ItemType");
            var quantityProp = property.FindPropertyRelative("Quantity");
            var durationProp = property.FindPropertyRelative("Duration");

            Rect NewRect() => new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            float heightPadding = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Рисуем Type
            EditorGUI.PropertyField(NewRect(), typeProp);
            position.y += heightPadding;

            // В зависимости от выбранного Type — показываем либо ItemName, либо ItemType
            if ((CraftIngredientType)typeProp.enumValueIndex == CraftIngredientType.ExactlyItem)
            {
                EditorGUI.PropertyField(NewRect(), itemNameProp);
                position.y += heightPadding;
            }
            else
            {
                EditorGUI.PropertyField(NewRect(), itemTypeProp);
                position.y += heightPadding;
            }

            // Всегда показываем количество
            EditorGUI.PropertyField(NewRect(), quantityProp);
            position.y += heightPadding;
            // Всегда показываем продолжительность крафта
            EditorGUI.PropertyField(NewRect(), durationProp);
            position.y += heightPadding;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProp = property.FindPropertyRelative("Type");
            int lines = 3; // Type + Quantity + Duration

            // Добавляем нужное поле
            if ((CraftIngredientType)typeProp.enumValueIndex == CraftIngredientType.ExactlyItem)
                lines++; // ItemName
            else
                lines++; // ItemType

            return lines * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }

}
#endif