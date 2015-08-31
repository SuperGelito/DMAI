using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AssemblyCSharp
{
	public class CSP
	{
		//CSP elements
		private List<Variable> vars;
		private List<OverFloorType> domain;
		private Dictionary<Vector2,List<Variable>> neighbours;
		private List<Assignment> assignments;
		private int nassignments {get {return assignments.Count();}}
		//Customized parameters
		int toleranceFW = 2;
		int toleranceFM = 2;

		/// <summary>
		/// Return False if violates any constraint
		/// </summary>
		/// <param name="var">Variable to check</param>
		/// <param name="valToCheck">Value to check</param>
		public bool Constraint(Assignment assignment)
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
		public CSP (List<Variable> varsCSP, List<OverFloorType> domainCSP)
		{
			this.vars = varsCSP;
			this.domain = domainCSP;
			neighbours = new Dictionary<Vector2, List<Variable>> ();
			assignments = new List<Assignment>();

			for(int i=0;i<varsCSP.Count();i++)
			{
				Variable v = varsCSP[i];
				neighbours[v.pos] = v.FilterNeighbours(varsCSP);
				v.remDomain = domainCSP;
			}
		}

		/// <summary>
		/// Creates the assignment and prunes other variables
		/// </summary>
		/// <param name="var">Variable.</param>
		/// <param name="val">Value.</param>
		public void AssignValue(Variable var,OverFloorType val)
		{
			//Create a new assignment
			Assignment assign = new Assignment (var, val);
			//Get variable prunes
			Search.AC3 (ref assign, neighbours [var.pos]);
			//Go over variable domain prunes to remove from variables
			foreach (var varPrune in assign.domainPrune.Keys) {
				List<OverFloorType> valuesToPrune = assign.domainPrune[varPrune];
				Variable varToPrune = vars.First(v=>v.pos == varPrune);
				foreach(var vPrune in valuesToPrune)
				{
					varToPrune.removeDomainElement(vPrune);
				}
			}
			//Save the assignment
	        assignments.Add (new Assignment (var, val));
		}

		/// <summary>
		/// Removes an assignment and recover pruned variables
		/// </summary>
		/// <param name="var">Variable.</param>
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

		public List<Variable> OrderVariablesByMinimumRemainingValues()
		{
			return vars.OrderBy (v => v.remDomain).ToList ();
		}

		public List<Assignment> OrderAssignmentsByLeastRestrictingValues(List<Assignment> assignments)
		{
			return assignments.OrderByDescending (a => ConflictAssignment (a)).ToList();
		}

		public int ConflictAssignment(Assignment assign)
		{
			int ret;
			AssignValue (assign.variable, assign.value);
			ret = vars.Sum (v => v.remDomain.Count());
			RemoveValue (assign.variable);
			return ret;
		}

		public bool Goal()
		{
			return vars.Count () == nassignments;
		}
	}

	public class Variable
	{
		public Vector2 pos;
		//public OverFloorType? val;
		public List<OverFloorType> remDomain;

		public Variable(Vector2 posVar)
		{
			this.pos = posVar;
		}

		public void removeDomainElement(OverFloorType elem)
		{
			remDomain.Remove (elem);
		}

		public void addDomainElement(OverFloorType elem)
		{
			remDomain.Add (elem);
		}

		public List<Variable> FilterNeighbours(List<Variable> variables)
		{
			return variables.Where(v => Math.Abs(v.pos.x - this.pos.x)== 1 || Math.Abs(v.pos.y - this.pos.y)== 1).ToList();
		}
	}

	public class Assignment
	{
		public Variable variable;
		public OverFloorType value;
		public Dictionary<Vector2,List<OverFloorType>> domainPrune;
		public Assignment(Variable var,OverFloorType val)
		{
			this.variable = var;
			this.value = val;
			domainPrune = new Dictionary<Vector2, List<OverFloorType>> ();
		}

		public void AddDomainPrune(Vector2 var,OverFloorType val)
		{
			if (!domainPrune.ContainsKey (var))
				domainPrune [var] = new List<OverFloorType> ();
			domainPrune [var].Add (val);
		}
	}

	public static class Search
	{
		public static bool AC3(ref Assignment assign,List<Variable> neighbours){
			return true;
		}
	}

}



