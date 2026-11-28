using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor.New
{
    public class DataAssignRow<T_HOLDER, T_CATEG, T_WRAPPER> : VisualElement
        where T_HOLDER : DataHolder<T_CATEG, T_WRAPPER>
        where T_CATEG : DataCategory<T_WRAPPER> 
        where T_WRAPPER : DataObject
    {
        [UxmlAttribute] public bool onlyAvailable = false;
        DropdownField categorySelect;
        DropdownField objectSelect;

        public DataAssignRow()
        {
            style.flexDirection = FlexDirection.Row;

            Add(categorySelect = new()
            {
            });

            Add(objectSelect = new()
            {
                
            });
        }

        T_HOLDER holder;
        SerializedProperty dataAssign;
        public void Bind(T_HOLDER holder, SerializedProperty dataAssign)
        {
            this.holder = holder;
            this.dataAssign = dataAssign;

            categorySelect.UnregisterAllRemovableCallbacks();
            objectSelect.UnregisterAllRemovableCallbacks();

            int categoryId = dataAssign.FindPropertyRelative(nameof(DataAssign.categoryId)).intValue;
            int objectId = dataAssign.FindPropertyRelative(nameof(DataAssign.objectId)).intValue;

            categorySelect.choices = holder.CategoryChoices(true);
            var category = holder.GetCategByID(categoryId);

            categorySelect.style.color = StyleKeyword.Null;
            objectSelect.style.color = StyleKeyword.Null;
            if (category == null)
            {
                MarkEmpty(categorySelect, categoryId);
                MarkEmpty(objectSelect, objectId);
             
                objectSelect.choices = new();
                objectSelect.enabledSelf = false;
            }
            else
            {
                categorySelect.value = category.Name;

                
                objectSelect.choices = category.ObjectChoices(onlyAvailable);
                objectSelect.enabledSelf = true;

                var dataObject = category.GetObjectByID(objectId);
                if(dataObject == null)
                {
                    MarkEmpty(objectSelect, objectId);
                }
                else
                {
                    objectSelect.value = dataObject.Name;
                }
            }

            categorySelect.RegisterCallback<ChangeEvent<string>>(CategoryChange, CallbackOptions.Removable);
            objectSelect.RegisterCallback<ChangeEvent<string>>(ObjectChange, CallbackOptions.Removable);
        }

        private void MarkEmpty(DropdownField field, int objectId)
        {
            if (objectId > 0)
            {
                field.style.color = Color.red;
                field.value = "Missing!";
            }
            else
                field.value = "None";
        }

        void CategoryChange(ChangeEvent<string> changeEvent)
        {
            if (changeEvent.newValue == changeEvent.previousValue)
                return;

            int newId = holder.GetCategIdFromName(changeEvent.newValue);
            Debug.Log(newId);
            dataAssign.FindPropertyRelative(nameof(DataAssign.categoryId)).intValue = newId;
            dataAssign.FindPropertyRelative(nameof(DataAssign.objectId)).intValue = 0;

            dataAssign.serializedObject.ApplyModifiedProperties();
            ClearObjectField(newId);
        }

        void ClearObjectField(int categoryId)
        {

            if (categoryId == 0)
            {
                objectSelect.enabledSelf = false;
            }
            else
            {
                objectSelect.enabledSelf = true;
                objectSelect.choices = holder.GetCategByID(categoryId)?.ObjectChoices();
            }
            objectSelect.SetValueWithoutNotify("None");
        }

        private void ObjectChange(ChangeEvent<string> changeEvent)
        {
            if (changeEvent.newValue == changeEvent.previousValue)
                return;
            int categId = dataAssign.FindPropertyRelative(nameof(DataAssign.categoryId)).intValue;
            var category = holder.GetCategByID(categId);
            if (category == null)
            {
                Debug.LogError("Category vanished!");
                dataAssign.FindPropertyRelative(nameof(DataAssign.objectId)).intValue = 0;
                ClearObjectField(0);
            }
            else
            {
                dataAssign.FindPropertyRelative(nameof(DataAssign.objectId)).intValue = category.GetObjectIDByName(changeEvent.newValue);
            }
            dataAssign.serializedObject.ApplyModifiedProperties();
        }
    }
}
