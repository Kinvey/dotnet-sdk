// Copyright (c) 2014, Kinvey, Inc. All rights reserved.
//
// This software is licensed to you under the Kinvey terms of service located at
// http://www.kinvey.com/terms-of-use. By downloading, accessing and/or using this
// software, you hereby accept such terms of service  (and any agreement referenced
// therein) and agree that you have read, understand and agree to be bound by such
// terms of service and are of legal age to agree to such terms with Kinvey.
//
// This software contains valuable confidential and proprietary information of
// KINVEY, INC and is subject to applicable licensing agreements.
// Unauthorized reproduction, transmission or distribution of this file and its
// contents is a violation of applicable laws.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ast = LinqExtender.Ast;
using LinqExtender;

namespace KinveyXamarin
{

	/// <summary>
	/// Kinvey query context, providing support for LINQ queries.
	/// </summary>
	public abstract class KinveyQueryContext<T> : ExpressionVisitor, IQueryContext<T>
	{
		/// <summary>
		/// The client.
		/// </summary>
		protected AbstractClient client;

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyQueryContext`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		public KinveyQueryContext(AbstractClient client){
			this.client = client;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="KinveyXamarin.KinveyQueryContext`1"/> class.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="writer">Writer.</param>
		public KinveyQueryContext (AbstractClient client, IQueryBuilder writer)
		{
			this.writer = writer;
			this.client = client;
		}

		/// <summary>
		/// Executes the current Linq query.
		/// </summary>
		/// <param name="exprssion"></param>
		/// <returns></returns>
		/// <param name="expression">Expression.</param>
		public IEnumerable<T> Execute(Ast.Expression expression)
		{
			writer.Write ("{");
			this.Visit(expression);
			writer.Write ("}");

			T[] ret = executeQuery (writer.GetFullString());
			writer.Reset ();

			return ret;

		}

		/// <summary>
		/// Visits the type expression.
		/// </summary>
		/// <returns>The type expression.</returns>
		/// <param name="typeExpression">Type expression.</param>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitTypeExpression(Ast.TypeExpression expression)
		{
			//this one is a no-op because we don't support typing, (one type per collection and no joins)

			return expression;
		}

		/// <summary>
		/// Visits the lambda expression.
		/// </summary>
		/// <returns>The lambda expression.</returns>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitLambdaExpression(Ast.LambdaExpression expression)
		{
			//another no-op, `where` clause is implicit in Mongo
			this.Visit(expression.Body);
			return expression;
		}

		/// <summary>
		/// Visits the binary expression.
		/// </summary>
		/// <returns>The binary expression.</returns>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitBinaryExpression(Ast.BinaryExpression expression)
		{
			//gte, lte, lt, gt, eq, ne, contains are binary expressions
			this.Visit(expression.Left);
			writer.Write(GetBinaryOperator(expression.Operator));
			this.Visit(expression.Right);

			return expression;
		}

		/// <summary>
		/// Visits the logical expression.
		/// </summary>
		/// <returns>The logical expression.</returns>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitLogicalExpression(Ast.LogicalExpression expression)
		{
			//And, Or are logical expressions

			if (expression.Operator == LogicalOperator.And) {
			
				this.Visit (expression.Left);

				writer.Write (",");

				this.Visit (expression.Right);

			} else {
				//it's an OR
				writer.Write ("$or:");
				writer.Write("[");
				writer.Write ("{");
				this.Visit (expression.Left);
				writer.Write ("},");
				writer.Write ("{");
				this.Visit (expression.Right);
				writer.Write ("}");
				writer.Write ("]");
			}

			return expression;
		}

		/// <summary>
		/// Visits the member expression.
		/// </summary>
		/// <returns>The member expression.</returns>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitMemberExpression(Ast.MemberExpression expression)
		{


			//expression.FullName returns `MyClass.MyField`, need to remove `MyClass.`
			//so find the first index of the . and then substring from one character after it.
			string withoutClassDec = expression.FullName.Substring (expression.FullName.IndexOf (".") + 1);

			writer.Write(String.Format("\"{0}\"", withoutClassDec));
			return expression;
		}

		/// <summary>
		/// Visits the literal expression.
		/// </summary>
		/// <returns>The literal expression.</returns>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitLiteralExpression(Ast.LiteralExpression expression)
		{
			//TODO literal expression is where type is implied by a constant's inline declaration -> string, "hey" or float, 10f
			WriteValue(expression.Type, expression.Value);
			return expression;
		}

		/// <summary>
		/// Visits the orderby expression.
		/// </summary>
		/// <returns>The orderby expression.</returns>
		/// <param name="expression">Expression.</param>
		public override Ast.Expression VisitOrderbyExpression(Ast.OrderbyExpression expression)
		{

			//ascending, descending
			string sort = "&sort={\"" + expression.Member.Name + "\":" + (expression.Ascending ? "1" : "-1") + "}";
			writer.Dangle (sort);

			return expression;
		}

		/// <summary>
		/// Gets the binary operator.
		/// </summary>
		/// <returns>The binary operator.</returns>
		/// <param name="operator">Operator.</param>
		private static string GetBinaryOperator(BinaryOperator @operator)
		{
			switch (@operator)
			{
			case BinaryOperator.Equal:
				return ":";
			}
			throw new ArgumentException("Invalid binary operator");
		}

		/// <summary>
		/// Writes the logical operator.
		/// </summary>
		/// <param name="logicalOperator">Logical operator.</param>
		private void WriteLogicalOperator(LogicalOperator logicalOperator)
		{
			WriteSpace();

			writer.Write(logicalOperator.ToString().ToUpper());


			WriteSpace();
		}

		/// <summary>
		/// Writes a space.
		/// </summary>
		private void WriteSpace()
		{
			writer.Write(" ");
		}

		/// <summary>
		/// Writes a new line.
		/// </summary>
		private void WriteNewLine()
		{
			writer.Write(Environment.NewLine);
		}

		/// <summary>
		/// Writes the token if reqiured by the expression.
		/// </summary>
		/// <param name="expression">Expression.</param>
		/// <param name="token">Token.</param>
		private void WriteTokenIfReq(Ast.LogicalExpression expression, Token token)
		{
			if (expression.IsChild)
			{
				WriteToken(token);
			}
		}

		/// <summary>
		/// Writes the token.
		/// </summary>
		/// <param name="token">Token.</param>
		private void WriteToken(Token token)
		{
			switch (token)
			{
			case Token.LeftParenthesis:
				writer.Write("(");
				break;
			case Token.RightParentThesis:
				writer.Write(")");
				break;
			}
		}

		/// <summary>
		/// Available tokens to write
		/// </summary>
		public enum Token
		{
			LeftParenthesis,
			RightParentThesis
		}

		/// <summary>
		/// Writes the value.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="value">Value.</param>
		private void WriteValue(TypeReference type, object value)
		{
			if (type.UnderlyingType == typeof(string))
				writer.Write(String.Format("\"{0}\"", value));
			else
				writer.Write(value);
		}

		/// <summary>
		/// Write the specified value.
		/// </summary>
		/// <param name="value">Value.</param>
		private void Write(string value)
		{
			writer.Write(value);
		}

		/// <summary>
		/// The query builder.
		/// </summary>
		public IQueryBuilder writer;

		/// <summary>
		/// The parameter.
		/// </summary>
		public bool parameter;

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <returns>The query.</returns>
		/// <param name="query">Query.</param>
		protected abstract T[] executeQuery(string query);
	}


}
