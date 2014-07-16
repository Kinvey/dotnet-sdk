using System;
using System.Linq.Expressions;

namespace KinveyXamarin
{
	internal class LocationFinder : ExpressionVisitor
	{
		private Expression expression;
		private List<string> locations;

		public LocationFinder(Expression exp)
		{
			this.expression = exp;
		}

		public List<string> Locations
		{
			get
			{
				if (locations == null)
				{
					locations = new List<string>();
					this.Visit(this.expression);
				}
				return this.locations;
			}
		}

		protected override Expression VisitBinary(BinaryExpression be)
		{
			if (be.NodeType == ExpressionType.Equal)
			{
				if (ExpressionTreeHelpers.IsMemberEqualsValueExpression(be, typeof(Place), "Name"))
				{
					locations.Add(ExpressionTreeHelpers.GetValueFromEqualsExpression(be, typeof(Place), "Name"));
					return be;
				}
				else if (ExpressionTreeHelpers.IsMemberEqualsValueExpression(be, typeof(Place), "State"))
				{
					locations.Add(ExpressionTreeHelpers.GetValueFromEqualsExpression(be, typeof(Place), "State"));
					return be;
				}
				else 
					return base.VisitBinary(be);
			}
			else 
				return base.VisitBinary(be);
		}
	}
}

