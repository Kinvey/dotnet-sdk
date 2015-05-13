using System;
using Remotion.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using System.Linq;
using Remotion.Linq.Parsing.Structure;
using System.Collections.ObjectModel;
using KinveyUtils;

namespace KinveyXamarin
{
	public class KinveyQueryExecutor : IQueryExecutor
	{

		// Set up a proeprty that will hold the current item being enumerated.
		public SampleDataSourceItem Current { get; private set; }

		private StringQueryBuilder writer;

		public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
		{
	
			// Create an expression that returns the current item when invoked.
			Expression currentItemExpression = Expression.Property(Expression.Constant(this), "Current");

			// Now replace references like the "i" in "select i" that refers to the "i" in "from i in items"
			var mapping = new QuerySourceMapping();
			mapping.AddMapping(queryModel.MainFromClause, currentItemExpression);
			queryModel.TransformExpressions(e =>
				ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences(e, mapping, true));


			writer = new StringQueryBuilder ();
		

			KinveyQueryVisitor visitor = new KinveyQueryVisitor(writer);

			writer.Write ("{");
			queryModel.Accept (visitor);
			writer.Write ("}");

			Logger.Log (writer.GetFullString ());


			// Create a lambda that takes our SampleDataSourceItem and passes it through the select clause
			// to produce a type of T.  (T may be SampleDataSourceItem, in which case this is an identity function.)
			var currentItemProperty = Expression.Parameter(typeof(SampleDataSourceItem));
			var projection = Expression.Lambda<Func<SampleDataSourceItem, T>>(queryModel.SelectClause.Selector, currentItemProperty);
			var projector = projection.Compile();



			// Pretend we're getting SampleDataSourceItems from somewhere...
			for (var i = 0; i < 10; i++)
			{
				// Set the current item so currentItemExpression can access it.
				Current = new SampleDataSourceItem
				{
					Name = "Name " + i,
					Description = "This describes the item in position " + i,
					lowercasetest  = "name " + i
				};

				// Use the projector to convert (if necessary) the current item to what is being selected and return it.
				yield return projector(Current);
			}
		}

		public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
		{
			var sequence = ExecuteCollection<T>(queryModel);
			return returnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
		}

		public T ExecuteScalar<T>(QueryModel queryModel)
		{
			throw new NotImplementedException();
		}
	}

	public class SampleDataSourceItem
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string lowercasetest {get; set;}
	}

	public class SampleQueryable<T> : QueryableBase<T>
	{
		public SampleQueryable(IQueryParser queryParser, IQueryExecutor executor)
			: base(new DefaultQueryProvider(typeof(SampleQueryable<>), queryParser, executor))
		{
		}

		public SampleQueryable(IQueryProvider provider, Expression expression)
			: base(provider, expression)
		{
		}
	}

	public class KinveyQueryVisitor : QueryModelVisitorBase {

		private IQueryBuilder writer;

		public KinveyQueryVisitor(IQueryBuilder builder){
			writer = builder;
		}

		public override void VisitQueryModel (QueryModel queryModel){
			base.VisitQueryModel (queryModel);
			Logger.Log ("visiting querymodel");
		}

		
		protected override void VisitBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
		{
			base.VisitBodyClauses (bodyClauses, queryModel);
			Logger.Log ("visiting body clause");
		}

		protected override void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
		{
			base.VisitOrderings (orderings, queryModel, orderByClause);

			Logger.Log ("visiting ordering clause");
		}

		protected override void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
		{
			base.VisitResultOperators (resultOperators, queryModel);
			Logger.Log ("visiting result clause");
		}

		public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index){
			base.VisitWhereClause (whereClause, queryModel, index);
			Logger.Log ("visiting where clause");
			if (whereClause.Predicate.NodeType.ToString ().Equals ("Equal")) {
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;

				writer.Write ("\"" + member.Member.Name + "\"");
				writer.Write (":");
				writer.Write (equality.Right);
			}else if (whereClause.Predicate.NodeType.ToString().Equals("AndAlso")){
				
			}else if (whereClause.Predicate.NodeType.ToString().Equals("OrElse")){

			} else {
				Logger.Log (whereClause.Predicate);
				Logger.Log (whereClause.Predicate.NodeType.ToString());

			}



		}

		public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index){
			base.VisitOrderByClause (orderByClause, queryModel, index);
			Logger.Log ("visiting orderby clause");
		}
//		public virtual void VisitAdditionalFromClause (AdditionalFromClause fromClause, QueryModel queryModel, int index);
//
//		public virtual void VisitGroupJoinClause (GroupJoinClause groupJoinClause, QueryModel queryModel, int index);
//
//		public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause);
//
//		public virtual void VisitJoinClause (JoinClause joinClause, QueryModel queryModel, int index);
//
//		public virtual void VisitMainFromClause (MainFromClause fromClause, QueryModel queryModel);
//
//		protected virtual void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause);
//
//		public virtual void VisitResultOperator (ResultOperatorBase resultOperator, QueryModel queryModel, int index);
//
//		protected virtual void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel);
//
//		public virtual void VisitSelectClause (SelectClause selectClause, QueryModel queryModel);
//

	}
}