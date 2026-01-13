using System.Collections.Generic;
using UnityEngine;

public class TaskPopulator : MonoBehaviour
{
    private void Start()
    {
        if (TaskManager.Instance != null)
        {
            PopulateTasks();
        }
    }

    private void PopulateTasks()
    {
        if (TaskManager.Instance == null) return;

        List<TaskData> tasks = new List<TaskData>();

        // Day 1
        tasks.Add(CreateTask("Talk to the person on the door", 1, 8, 0, "TalkDoor1", new[] { ("Bashir", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Prepare breakfast", 1, 9, 0, "Breakfast1", new[] { ("Sahil", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Take a shower", 1, 10, 0, "Shower1", null));
        tasks.Add(CreateTask("Scavenge backyard for materials", 1, 11, 0, "ScavengeBack1", null));
        tasks.Add(CreateTask("Eat breakfast", 1, 11, 0, "EatBreakfast1", null));
        tasks.Add(CreateTask("Talk to Sahil", 1, 13, 0, "TalkSahil1", new[] { ("Bashir", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Talk to Bashir", 1, 13, 0, "TalkBashir1", new[] { ("Sahil", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Craft donation box", 1, 14, 0, "CraftDonation1", new[] { ("Bashir", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Scavenge bins for materials", 1, 14, 0, "ScavengeBins1", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Plant seeds", 1, 15, 0, "PlantSeeds1", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Water crops", 1, 15, 0, "WaterCrops1", new[] { ("Sahil", CharacterStats.PrimaryAttribute.Learning, 5) }));
        tasks.Add(CreateTask("Place donation box", 1, 16, 0, "PlaceDonation1", null, TaskData.TaskType.ObjectActivation));
        tasks.Add(CreateTask("Prepare dinner", 1, 18, 0, "Dinner1", null));
        tasks.Add(CreateTask("Check donation box", 1, 18, 0, "CheckDonation1", null));
        tasks.Add(CreateTask("Eat dinner", 1, 19, 0, "EatDinner1", null));
        tasks.Add(CreateTask("Check donation box (Evening)", 1, 18, 0, "CheckDonation1E", null));
        tasks.Add(CreateTask("Paint living room walls", 1, 20, 0, "PaintLiving1", new[] { ("Bashir", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Paint kitchen walls", 1, 20, 0, "PaintKitchen1", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Sleep", 1, 21, 0, "Sleep1", null));

        // Day 2
        tasks.Add(CreateTask("Talk to the person on the door", 2, 9, 0, "TalkDoor2", new[] { ("Bashir", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Talk to Aisha", 2, 9, 0, "TalkAisha2", new[] { ("Aisha", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Talk to Sagar", 2, 9, 0, "TalkSagar2", new[] { ("Sagar", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Make breakfast", 2, 9, 0, "Breakfast2", null));
        tasks.Add(CreateTask("Eat breakfast (all residents)", 2, 10, 0, "EatBreakfast2", null));
        tasks.Add(CreateTask("Scavenge front yard for materials", 2, 11, 0, "ScavengeFront2", null));
        tasks.Add(CreateTask("Water the crops", 2, 11, 0, "WaterCrops2", new[] { ("Aisha", CharacterStats.PrimaryAttribute.Learning, 5) }));
        tasks.Add(CreateTask("Help your mom water the crops", 2, 11, 0, "HelpWater2", new[] { ("Sagar", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Craft windmill", 2, 12, 0, "CraftWindmill2", new[] { ("Bashir", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Place windmill", 2, 12, 0, "PlaceWindmill2", null, TaskData.TaskType.ObjectActivation));
        tasks.Add(CreateTask("Play in the backyard", 2, 12, 0, "PlayBackyard2", new[] { ("Sagar", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Check donation box (Afternoon)", 2, 15, 0, "CheckDonation2A", null));
        tasks.Add(CreateTask("Scavenge bins for materials", 2, 14, 0, "ScavengeBins2", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Place solar panels", 2, 15, 0, "PlaceSolar2", new[] { ("Bashir", CharacterStats.PrimaryAttribute.WorkReadiness, 5), ("Bashir", CharacterStats.PrimaryAttribute.Stability, 5) }, TaskData.TaskType.ObjectActivation));
        tasks.Add(CreateTask("Craft sewing machine", 2, 15, 0, "CraftSewing2", new[] { ("Sahil", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Place sewing machine", 2, 15, 0, "PlaceSewing2", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }, TaskData.TaskType.ObjectActivation));
        tasks.Add(CreateTask("Help Sagar study", 2, 16, 0, "HelpStudy2", new[] { ("Bashir", CharacterStats.PrimaryAttribute.WorkReadiness, 5), ("Aisha", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Check donation box (Afternoon 2)", 2, 16, 0, "CheckDonation2A2", null));
        tasks.Add(CreateTask("Sew curtain", 2, 16, 0, "SewCurtain2", new[] { ("Aisha", CharacterStats.PrimaryAttribute.Learning, 5) }));
        tasks.Add(CreateTask("Place new curtain", 2, 16, 0, "PlaceCurtain2", new[] { ("Aisha", CharacterStats.PrimaryAttribute.WorkReadiness, 5), ("Bashir", CharacterStats.PrimaryAttribute.Stability, 5) }, TaskData.TaskType.ObjectActivation));
        tasks.Add(CreateTask("Study", 2, 16, 0, "Study2", new[] { ("Sagar", CharacterStats.PrimaryAttribute.Learning, 5) }));
        tasks.Add(CreateTask("Play in the backyard (Evening)", 2, 17, 0, "PlayBackyard2E", new[] { ("Sagar", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Check donation box (Evening)", 2, 17, 0, "CheckDonation2E", null));
        tasks.Add(CreateTask("Cook dinner", 2, 18, 0, "CookDinner2", null));
        tasks.Add(CreateTask("Eat dinner (all residents)", 2, 19, 0, "EatDinner2", null));
        tasks.Add(CreateTask("Sleep", 2, 20, 0, "Sleep2", null));

        // Day 3
        tasks.Add(CreateTask("Make breakfast", 3, 9, 0, "Breakfast3", null));
        tasks.Add(CreateTask("Help Sagar study", 3, 11, 0, "HelpStudy3", new[] { ("Aisha", CharacterStats.PrimaryAttribute.WorkReadiness, 5), ("Aisha", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Study", 3, 11, 0, "Study3", new[] { ("Sagar", CharacterStats.PrimaryAttribute.Learning, 5) }));
        tasks.Add(CreateTask("Check donation box", 3, 12, 0, "CheckDonation3", new[] { ("Bashir", CharacterStats.PrimaryAttribute.Stability, 5) }));
        tasks.Add(CreateTask("Scavenge bins in the front yard", 3, 12, 0, "ScavengeFront3", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Craft canvas frame", 3, 13, 0, "CraftCanvas3", new[] { ("Sahil", CharacterStats.PrimaryAttribute.Trust, 5), ("Aisha", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Play in backyard", 3, 13, 0, "PlayBackyard3", null));
        tasks.Add(CreateTask("Sew cloth for canvas", 3, 14, 0, "SewCloth3", new[] { ("Aisha", CharacterStats.PrimaryAttribute.WorkReadiness, 5), ("Aisha", CharacterStats.PrimaryAttribute.Stability, 5), ("Aisha", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Place canvas", 3, 14, 0, "PlaceCanvas3", new[] { ("Bashir", CharacterStats.PrimaryAttribute.Stability, 5) }, TaskData.TaskType.ObjectActivation));
        tasks.Add(CreateTask("Harvest crops", 3, 15, 0, "Harvest3", new[] { ("Bashir", CharacterStats.PrimaryAttribute.WorkReadiness, 5), ("Bashir", CharacterStats.PrimaryAttribute.Stability, 5), ("Sahil", CharacterStats.PrimaryAttribute.Stability, 5) }));
        tasks.Add(CreateTask("Learn how to paint", 3, 15, 0, "LearnPaint3", new[] { ("Sagar", CharacterStats.PrimaryAttribute.Learning, 5), ("Sagar", CharacterStats.PrimaryAttribute.Stability, 5) }));
        tasks.Add(CreateTask("Water plants", 3, 16, 0, "WaterPlants3", null));
        tasks.Add(CreateTask("Paint bedroom walls", 3, 16, 0, "PaintBedroom3", new[] { ("Sahil", CharacterStats.PrimaryAttribute.WorkReadiness, 5) }));
        tasks.Add(CreateTask("Paint kitchen walls", 3, 16, 0, "PaintKitchen3", new[] { ("Aisha", CharacterStats.PrimaryAttribute.Trust, 5) }));
        tasks.Add(CreateTask("Water plants (Evening)", 3, 16, 0, "WaterPlants3E", null));
        tasks.Add(CreateTask("Cook dinner", 3, 18, 0, "CookDinner3", null));
        tasks.Add(CreateTask("Eat dinner (all residents)", 3, 19, 0, "EatDinner3", new[] { ("Bashir", CharacterStats.PrimaryAttribute.Stability, 5), ("Sahil", CharacterStats.PrimaryAttribute.Stability, 5), ("Aisha", CharacterStats.PrimaryAttribute.Stability, 5), ("Sagar", CharacterStats.PrimaryAttribute.Stability, 5) }));
        tasks.Add(CreateTask("Sleep", 3, 20, 0, "Sleep3", null));

        // Add all to TaskManager
        TaskManager.Instance.SetAllTasks(tasks);
        Debug.Log($"[TaskPopulator] Successfully populated {tasks.Count} tasks into TaskManager.");
        
        // Trigger update
        if (TaskManager.Instance != null && TimeManager.Instance != null)
        {
            TaskManager.Instance.UpdateDayTasks(TimeManager.Instance.days);
            Debug.Log($"[TaskPopulator] Triggered UpdateDayTasks for Day {TimeManager.Instance.days}.");
        }
    }

    private TaskData CreateTask(string desc, int day, int hour, int minute, string target, (string charName, CharacterStats.PrimaryAttribute attr, int amt)[] effects, TaskData.TaskType type = TaskData.TaskType.Interaction, TaskData.CharacterName requiredChar = TaskData.CharacterName.None)
    {
        TaskData task = ScriptableObject.CreateInstance<TaskData>();
        task.taskDescription = desc;
        task.day = day;
        task.hour = hour;
        task.minute = minute;
        task.requirementTarget = target;
        task.taskType = type;
        task.requiredCharacter = requiredChar;
        task.statEffects = new List<TaskData.StatEffect>();
    
        if (effects != null)
        {
            foreach (var e in effects)
            {
                // Parse the string name into the CharacterName enum
                TaskData.CharacterName charEnum = TaskData.CharacterName.None;
                System.Enum.TryParse(e.charName, true, out charEnum);

                task.statEffects.Add(new TaskData.StatEffect 
                { 
                    characterName = charEnum, 
                    attribute = e.attr, 
                    amount = e.amt 
                });
            }
        }
    
        return task;
    }
}
