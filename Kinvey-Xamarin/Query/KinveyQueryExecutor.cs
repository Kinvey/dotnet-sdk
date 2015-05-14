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
	public class KinveyQueryExecutor<K> : IQueryExecutor
	{
		
		public StringQueryBuilder writer;
		public KinveyQueryable<K> queryable;

		public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
		{
		
			writer.Reset ();

			KinveyQueryVisitor visitor = new KinveyQueryVisitor(writer);

			writer.Write ("{");
			queryModel.Accept (visitor);
			writer.Write ("}");

			Logger.Log (writer.GetFullString ());

			K[] results =  queryable.executeQuery (writer.GetFullString ());

			yield return default(T);
		
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
		public string ID {get; set;}
		public bool IsAvailable { get; set;}
	}

	public class KinveyQueryable<T> : QueryableBase<T>
	{
		public StringQueryBuilder writer;

		public KinveyQueryable(IQueryParser queryParser, IQueryExecutor executor)
			: base(new DefaultQueryProvider(typeof(KinveyQueryable<>), queryParser, executor))
		{
			var kExecutor = executor as KinveyQueryExecutor<T>;
			if (kExecutor != null) {
				writer = new StringQueryBuilder ();
				kExecutor.writer = writer;
				kExecutor.queryable = this;
			}

		}

		public KinveyQueryable(IQueryProvider provider, Expression expression)
			: base(provider, expression)
		{
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="query">Query.</param>
		public virtual T[] executeQuery(string query){
			Logger.Log ("can't execute a query without overriding this method!");
			return default(T[]);
		}
	}

	public class KinveyQueryVisitor : QueryModelVisitorBase {

		private IQueryBuilder writer;

		public KinveyQueryVisitor(IQueryBuilder builder){
			writer = builder;
		}

		public override void VisitQueryModel (QueryModel queryModel){
			base.VisitQueryModel (queryModel);
//			Logger.Log ("visiting querymodel");
		}

		
		protected override void VisitBodyClauses (ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
		{
			base.VisitBodyClauses (bodyClauses, queryModel);
//			Logger.Log ("visiting body clause");
		}

		protected override void VisitOrderings (ObservableCollection<Ordering> orderings, QueryModel queryModel, OrderByClause orderByClause)
		{
			base.VisitOrderings (orderings, queryModel, orderByClause);

//			Logger.Log ("visiting ordering clause");
			foreach (var ordering in orderings) {
				var member = ordering.Expression as MemberExpression;


				string sort = "&sort={\"" + member.Member.Name + "\":" + (ordering.OrderingDirection.ToString().Equals("Asc") ? "1" : "-1") + "}";
				writer.Dangle (sort);

//				Logger.Log (ordering.OrderingDirection);
//				ordering.OrderingDirection
			}
		}

//		protected override void VisitResultOperators (ObservableCollection<ResultOperatorBase> resultOperators, QueryModel queryModel)
//		{
//			base.VisitResultOperators (resultOperators, queryModel);
//			Logger.Log ("visiting result clause");
//		}

		public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index){
			base.VisitWhereClause (whereClause, queryModel, index);
//			Logger.Log ("visiting where clause");
			if (whereClause.Predicate.NodeType.ToString ().Equals ("Equal")) {
				BinaryExpression equality = whereClause.Predicate as BinaryExpression;
				var member = equality.Left as MemberExpression;

				writer.Write ("\"" + member.Member.Name + "\"");
				writer.Write (":");
				writer.Write (equality.Right);
			}else if (whereClause.Predicate.NodeType.ToString().Equals("AndAlso")){
				BinaryExpression and = whereClause.Predicate as BinaryExpression;
				//recursively traverse tree
//				var rightSide = and.Right as BinaryExpression;
//				if (rightSide != null){
//					
//				}
				VisitWhereClause(new WhereClause(and.Right), queryModel, index);
				writer.Write (",");
				VisitWhereClause(new WhereClause(and.Left), queryModel, index);
					


//				Logger.Log (and.Right.ToString());
//				Logger.Log (and.Left.ToString());


			}else if (whereClause.Predicate.NodeType.ToString().Equals("OrElse")){
				BinaryExpression or = whereClause.Predicate as BinaryExpression;

				writer.Write ("$or:");
				writer.Write("[");
				writer.Write ("{");
				VisitWhereClause (new WhereClause(or.Left), queryModel, index);
				writer.Write ("},");
				writer.Write ("{");
				VisitWhereClause (new WhereClause(or.Right), queryModel, index);
				writer.Write ("}");
				writer.Write ("]");


//				Logger.Log (or.Right.ToString());
//				Logger.Log (or.Left.ToString());

			} else {
//				Logger.Log (whereClause.Predicate);
//				Logger.Log (whereClause.Predicate.NodeType.ToString());

			}



		}

//		public override void VisitOrderByClause (OrderByClause orderByClause, QueryModel queryModel, int index){
//			base.VisitOrderByClause (orderByClause, queryModel, index);
//			Logger.Log ("visiting orderby clause");
//			foreach (var ordering in orderByClause.Orderings) {
//				Logger.Log (ordering.Expression);
//			}
//
//		}
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