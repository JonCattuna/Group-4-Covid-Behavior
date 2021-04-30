using System;
using System.Collections.Generic;

public static class POP {
    // Goal booleans, loop ends when all are true
    static bool visitedClassroom = false;
    static bool visitedCafeteria = false;
    static bool visitedTable = false;
    static bool visitedBathroom = false;

    // Current Conditions
    static bool handsClean = false;
    static bool handsDirty = false;
    static bool gotFood = false;
    static bool startDay = true;

    static string selectSubgoal(HashSet<String> subgoals, int numSubGoals) {
        //takes in an unused precondition
        Random rnd = new Random();
        int num  = rnd.Next(0, numSubGoals);
        int counter = 0;
        foreach (string thing in subgoals) {
            if (counter == num) return thing;
            counter++;
        }
        return "No Sugoal";
    }

    static string chooseOperator(string subGoal, Tuple<String, String> goToClass, Tuple<String, String> goToCafeteria, Tuple<String, String> goToTable, Tuple<String, String> goToBathroom) {
        if (subGoal == "goToClass") return goToClass.Item1;
        if (subGoal == "goToCafeteria") return goToCafeteria.Item1;
        if (subGoal == "goToTable") return goToTable.Item1;
        if (subGoal == "goToBathroom") return goToBathroom.Item1;
        return "No Operator";
    }

    static bool checkForThreats(String operatorToDo, bool handsClean, bool handsDirty, bool gotFood,  bool worried) {
        if ((operatorToDo == "Classroom") && (gotFood == false)) {
            if ((handsClean == true) || (!worried)) return true;
        } else if ((operatorToDo == "Bathroom") && (gotFood == false)) {
            if ((handsDirty == true) || (!worried)) return true;
        } else if ((operatorToDo == "Cafeteria") && (gotFood == false)) {
            if ((handsClean == true) || (!worried)) return true;
        } else if ((operatorToDo == "Table") && (gotFood == true)) {
            if ((handsClean == true) || (!worried)) return true;
        } else {
            return false;
        }
        return false;
    }

    public static List<String> popAlgo(bool covidConscious) {
        // Preconditions
        var goToClass = Tuple.Create("Classroom", "handsClean");
        var goToCafeteria = Tuple.Create("Cafeteria", "handsClean");
        var goToTable = Tuple.Create("Table", "handsClean");
        var goToBathroom = Tuple.Create("Bathroom", "handsDirty");

        var subgoals = new HashSet<String>() { "goToClass", "goToCafeteria", "goToTable", "goToBathroom" };
        var Preconditions = new HashSet<String>() { "handsClean", "handsDirty" };
        var Postconditions = new HashSet<String>() { "visitedClassroom", "visitedCafeteria", "visitedTable", "visitedBathroom", "visitedOutside" };
        var Locations = new HashSet<String>() { "Cafeteria", "Classroom", "Outside", "Bathroom", "Table" };

        // List to keep track of order
        var order = new List<string>();

        int subGoalCounter = 4;

        //Main Loop
        while (!(visitedClassroom == true && visitedBathroom == true && visitedCafeteria && visitedTable == true)) {

            if ((startDay == true)) {
                //order.Add("Outside");
                
                handsClean = true;
                handsDirty = false;
                startDay = false;

            } else {

                //Console.WriteLine("This ran");

                string subgoalToDo = selectSubgoal(subgoals, subGoalCounter);
                //Console.WriteLine(subgoalToDo);
                string operatorToDo = chooseOperator(subgoalToDo, goToClass, goToCafeteria, goToTable, goToBathroom);
                bool resolved = checkForThreats(operatorToDo, handsClean, handsDirty, gotFood, covidConscious);

                if (resolved == true) {
                    order.Add(operatorToDo);
                    if (operatorToDo == "Bathroom") {
                        handsClean = true;
                        handsDirty = false;
                        if (!covidConscious) subgoals.Remove("goToBathroom");
                        visitedBathroom = true;
                    } else if (operatorToDo == "Table") {
                        handsClean = false;
                        handsDirty = true;
                        visitedTable = true;
                        gotFood = false;
                        subgoals.Remove("goToTable");
                        subGoalCounter--;
                    } else if (operatorToDo == "Cafeteria") {
                        visitedCafeteria = true;
                        gotFood = true;
                        subgoals.Remove("goToCafeteria");
                        subGoalCounter--;
                    } else if(operatorToDo == "Classroom") {
                        handsClean = false;
                        handsDirty = true;
                        visitedClassroom = true;
                        subgoals.Remove("goToClass");
                        subGoalCounter--;
                    }
                }
                
            } 

            /*
            var precondition = selectSubgoal();

            if (taskDone(task, precondition) == true) {
                go(current, location);
            }*/

        }

        if (visitedClassroom == true && visitedBathroom == true && visitedCafeteria && visitedTable == true) order.Add("Outside");

        //Console.WriteLine();
        //order.ForEach(Console.WriteLine);
        //Console.WriteLine();

        return order;

    }

    static void Main(string[] args) {

        var order = new List<string>();

        order = popAlgo(false); // false is for covid ignorance, true is for covid awareness
    }

}

