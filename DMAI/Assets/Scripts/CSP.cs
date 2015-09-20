using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CSPNamespace
{
    /// <summary>
    /// Class to manage a type of problem of variable assignment based on a closed domain and restrictions, contraint satisfaction problem
    /// </summary>
	public class CSP
	{
		//CSP elements
		private List<Variable> vars;
		private List<OverFloorType> domain;
		private Dictionary<Vector2,List<Variable>> neighbours;
		private List<Assignment> assignments;
		private int nassignments {get {return assignments.Count();}}
        //CSP Parameters
        public bool MAC3 = true;
		//Customized parameters to determine conflicts
		int toleranceFW;
        int toleranceFM;

		/// <summary>
		/// Return False if violates any constraint
		/// </summary>
		/// <param name="var">Variable to check</param>
		/// <param name="valToCheck">Value to check</param>
		public bool ValidateConstraints(Assignment assignment)
		{
			//This filter only applies to check when are we setting a tile with ground or mud
			if (assignment.value != OverFloorType.Wall)
			//Look that not all neighbours are walls
			if (neighbours [assignment.variable.pos].All (nv => assignment.value == OverFloorType.Wall))
				return false;


			//Get number of elements of each val in variables
			int countFloor = assignments.Count (a => a.value == OverFloorType.Floor);
			int countWall = assignments.Count (a => a.value == OverFloorType.Wall);
			int countMud = assignments.Count (a => a.value == OverFloorType.Mud);

			//Look if the tolerance between Floor and Wall is valid
			int difFW = Math.Abs (countFloor - countWall);
			if (difFW > toleranceFW)
				return false;
			//Look if the tolerance between Floor and Mud is valid
			int difFM = Math.Abs (countFloor - countMud);
			if (difFM > toleranceFM)
				return false;

			return true;
		}

		/// <summary>
		/// Initializes a new instance of the CSP/> class.
		/// </summary>
		/// <param name="varsCSP">Variables</param>
		/// <param name="domainCSP">Domain</param>
		public CSP (List<Variable> varsCSP, List<OverFloorType> domainCSP,int toleranceFW,int toleranceFM)
		{
			this.vars = varsCSP;
			this.domain = domainCSP;
			neighbours = new Dictionary<Vector2, List<Variable>> ();
			assignments = new List<Assignment>();
            this.toleranceFM = toleranceFM;
            this.toleranceFW = toleranceFW;
			for(int i=0;i<varsCSP.Count();i++)
			{
				Variable v = varsCSP[i];
				neighbours[v.pos] = v.FilterNeighbours(varsCSP);
				v.remDomain = domain;
                if (v.val.HasValue)
                    assignments.Add(new Assignment(v, v.val.Value));
			}
		}

		/// <summary>
		/// Creates the assignment and prunes other variables
		/// </summary>
		/// <param name="var">Variable.</param>
		/// <param name="val">Value.</param>
        /// <returns>True if the assignment is legal</returns>
		public bool AssignValue(Variable var,OverFloorType val)
		{
            
			//Create a new assignment
			Assignment assign = new Assignment (var, val);
            //Get variable prunes
            var validAssignment = !MAC3 || Search.MAC3(ref assign, this);
            if (validAssignment)
            { 
                //Go over variable domain prunes to remove from variables
                foreach (var varPrune in assign.domainPrune.Keys)
                {
                    List<OverFloorType> valuesToPrune = assign.domainPrune[varPrune];
                    Variable varToPrune = vars.First(v => v.pos == varPrune);
                    foreach (var vPrune in valuesToPrune)
                    {
                        varToPrune.removeDomainElement(vPrune);
                    }
                }
                //Save the assignment
                assignments.Add(assign);
            }
            return validAssignment;
        }

		/// <summary>
		/// Removes an assignment and recover pruned variables
		/// </summary>
		/// <param name="var">Removes the assignment for this variable and adds pruned variables.</param>
		public void RemoveValue(Variable var)
		{
			Vector2 varPos = var.pos;
			Assignment assign = assignments.First (a => a.variable.pos == varPos);
			foreach (var varPrune in assign.domainPrune.Keys) {
				List<OverFloorType> valuesPruned = assign.domainPrune[varPrune];
				Variable varToUnprune = vars.First(v=>v.pos == varPrune);
				foreach(var vPrune in valuesPruned)
				{
					varToUnprune.addDomainElement(vPrune);
				}
			}
			assignments.Remove (assign);
		}

        /// <summary>
        /// Return the variable with less values to be assigned
        /// </summary>
        /// <returns>A list of variables ordered by remaining values</returns>
		public List<Variable> OrderVariablesByMinimumRemainingValues()
		{
			return vars.Where(v=> !this.isVariableAssigned(v)).OrderBy (v => v.remDomain).ToList ();
		}
        /// <summary>
        /// Return the value that will restrict less the assignment of future variables
        /// </summary>
        /// <param name="var">Variable to get possible assignments from</param>
        /// <returns>List with all possible assignments for the variable order by least restraining value</returns>
		public List<Assignment> OrderAssignmentsByLeastRestrictingValues(Variable var)
		{
            List<Assignment> remAssignments = new List<Assignment>();
            foreach (var remValue in var.remDomain)
            {
                remAssignments.Add(new Assignment(var, remValue));
            }

			return assignments.OrderByDescending (a => ConflictAssignment (a)).ToList();
		}

        /// <summary>
        /// Return the number of remaining values that the assignment will let after being executed
        /// </summary>
        /// <param name="assign">Assignment to be evaluated </param>
        /// <returns>Number of remaining domains values in all remaining variables</returns>
		public int ConflictAssignment(Assignment assign)
		{
			int ret;
			AssignValue (assign.variable, assign.value);
			ret = vars.Sum (v => v.remDomain.Count());
			RemoveValue (assign.variable);
			return ret;
		}

        /// <summary>
        /// Evaluates if all variables has been assigned
        /// </summary>
        /// <returns>True if goal state is reached</returns>
		public bool Goal()
		{
			return vars.Count () == nassignments;
		}

        /// <summary>
        /// Return the list of neighbours of a variable
        /// </summary>
        /// <param name="var">Variable to get neighbours from</param>
        /// <returns>List of neighbours</returns>
        public List<Variable> GetNeighbours(Variable var)
        {
            return neighbours[var.pos];
        }

        /// <summary>
        /// Removes a available value from a variable
        /// </summary>
        /// <param name="var"></param>
        /// <param name="value"></param>
        public void RemoveDomainElementFromVariable(Variable var, OverFloorType value)
        {
            this.vars.First(v=>v.pos == var.pos).removeDomainElement(value);
        }

        /// <summary>
        /// Return if a variable has some assignment
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public bool isVariableAssigned(Variable var)
        {
            return assignments.Any(a => a.variable.pos == var.pos);
        }

        /// <summary>
        /// Return the assignments
        /// </summary>
        /// <returns></returns>
        public List<Assignment> GetAssignments()
        {
            return this.assignments;
        }

	}

    /// <summary>
    /// This class represents a variable to be filled with some value
    /// </summary>
	public class Variable
	{
		public Vector2 pos;
		public OverFloorType? val;
		public List<OverFloorType> remDomain;

		public Variable(Vector2 posVar)
		{
			this.pos = posVar;
            this.val = null;
		}

        public Variable(Vector2 posVar,OverFloorType val)
        {
            this.pos = posVar;
            this.val = val;
        }
        /// <summary>
        /// Removes a value from the domain
        /// </summary>
        /// <param name="elem">Value to be removed</param>
		public void removeDomainElement(OverFloorType elem)
		{
			remDomain.Remove (elem);
		}
        /// <summary>
        /// Add a value to the domain
        /// </summary>
        /// <param name="elem">Value to be added to the domain</param>
		public void addDomainElement(OverFloorType elem)
		{
			remDomain.Add (elem);
		}

        /// <summary>
        /// Filter a list of variables to get the neighbours of the variable
        /// </summary>
        /// <param name="variables">Variables to be filtered</param>
        /// <returns>List of neighbours</returns>
		public List<Variable> FilterNeighbours(List<Variable> variables)
		{
			return variables.Where(v => Math.Abs(v.pos.x - this.pos.x)== 1 || Math.Abs(v.pos.y - this.pos.y)== 1).ToList();
		}
	}
    /// <summary>
    /// Class representing the assignment of a value to a variable
    /// </summary>
	public class Assignment
	{
		public Variable variable;
		public OverFloorType value;
        //List of variables removed from a variable
        public Dictionary<Vector2,List<OverFloorType>> domainPrune;

		public Assignment(Variable var,OverFloorType val)
		{
			this.variable = var;
			this.value = val;
			domainPrune = new Dictionary<Vector2, List<OverFloorType>> ();
		}
        /// <summary>
        /// Add a prune of a value to a variable
        /// </summary>
        /// <param name="var">Variable to be pruned</param>
        /// <param name="val">Value that will be pruned</param>
		public void AddDomainPrune(Vector2 var,OverFloorType val)
		{
			if (!domainPrune.ContainsKey (var))
				domainPrune [var] = new List<OverFloorType> ();
			domainPrune [var].Add (val);
		}
	}

    /// <summary>
    /// Class that contains the methods that will be reponsible of the variables and values iteration
    /// </summary>
	public static class Search
	{
        /// <summary>
        /// Maintenance Arc Consistency, this algorithm will check how many domain prunes are done when a assignemnt is executed
        /// </summary>
        /// <param name="assign">Assignment to be evaluated and updated</param>
        /// <param name="neighbours">List of neighbours to start with (difference between AC3 and MAC3)</param>
        /// <returns></returns>
		public static bool MAC3(ref Assignment assign,CSP problem){
            //Disable Arc consistency
            problem.MAC3 = false;
            //Add the assignment to the problem
            problem.AssignValue(assign.variable, assign.value);
            List<Variable> queueToCheck = new List<Variable>();
            queueToCheck.Add(assign.variable);
            //Create a new CSP with only neighbours variables to apply assign and remove values logic
            while(queueToCheck.Any()) 
            {
                //Get the first variable
                var variable = queueToCheck.First();
                //Get the neighbours of the variable
                List<Variable> neighbours = problem.GetNeighbours(variable);
                foreach (var neigh in neighbours)
                {
                    //Get the list of values to delete for conflict
                    List<OverFloorType> valuesToPrune = new List<OverFloorType>();
                    foreach (var remVal in neigh.remDomain)
                    {
                        if (!problem.isVariableAssigned(neigh) && problem.ValidateConstraints(new Assignment(neigh, remVal)))
                        {
                            valuesToPrune.Add(remVal);
                        }
                    }
                    //Remove the value from the variable domain in temporal CSP and add to the domain prune
                    foreach (var prunedVal in valuesToPrune)
                    {
                        problem.RemoveDomainElementFromVariable(neigh, prunedVal);
                        neigh.removeDomainElement(prunedVal);
                        assign.AddDomainPrune(neigh.pos, prunedVal);
                    }
                    //Check if variable lost all domain elements
                    if (neigh.remDomain.Any())
                    {
                        queueToCheck.Add(neigh);
                    }
                    else
                        return false;
                }   
            }

            return true;
        }

        /// <summary>
        /// Loop all variables and values searching for a solution for the CSP
        /// </summary>
        /// <param name="problem"></param>
        /// <returns></returns>
        public static List<Assignment> RecursiveBacktrackingSearch(CSP problem)
        {
            if (problem.Goal())
                return problem.GetAssignments();
            //Loop unasigned
            foreach (var variable in problem.OrderVariablesByMinimumRemainingValues())
            {
                foreach (var assign in problem.OrderAssignmentsByLeastRestrictingValues(variable))
                {
                    //Check if the assignment is valid
                    if (problem.ValidateConstraints(assign))
                    {
                        bool validResult = problem.AssignValue(assign.variable, assign.value);

                        if (!validResult)
                            return null;

                        var ret = RecursiveBacktrackingSearch(problem);

                        if (ret != null)
                            return ret;
                        else
                            problem.RemoveValue(assign.variable);
                    }
                }
            }

            return null;
        }
	}

}



