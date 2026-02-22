using UnityEditor;

[CustomEditor(typeof(TaskData))]
public class TaskDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        SerializedProperty timedTaskProp = serializedObject.FindProperty("timedTask");
        EditorGUILayout.PropertyField(timedTaskProp);
        
        if (timedTaskProp.boolValue)
        {
            SerializedProperty timeConditionTypeProp = serializedObject.FindProperty("timeConditionType");
            EditorGUILayout.PropertyField(timeConditionTypeProp);

            if (timeConditionTypeProp.enumValueIndex == (int)TaskData.TimeCondition.FromActivation)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("completeIn"));
            }
            else if (timeConditionTypeProp.enumValueIndex == (int)TaskData.TimeCondition.ByDayHourMinute)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("completeByDay"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("completeByHour"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("completeByMinute"));
            }
        }

        SerializedProperty taskTypeProp = serializedObject.FindProperty("taskType");
        EditorGUILayout.PropertyField(taskTypeProp);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("requirementTarget"));
        
        if (taskTypeProp.enumValueIndex == (int)TaskData.TaskType.Cooking || 
            taskTypeProp.enumValueIndex == (int)TaskData.TaskType.Crafting)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requirementProduct"));
        }

        DrawPropertiesExcluding(serializedObject, "m_Script", "timedTask", "timeConditionType", "completeByDay", "completeByHour", "completeByMinute", "completeIn", "taskType", "requirementTarget", "requirementProduct");

        serializedObject.ApplyModifiedProperties();
    }
}
