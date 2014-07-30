/*
Copyright (c) 2007- 2010 LinqExtender Toolkit Project.

Permission is hereby granted, free of charge, to any person obtaining a 
copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation 
the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the 
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included 
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE.
*/

using System;
using LinqExtender;
using Ast = LinqExtender.Ast;


namespace AndroidTestDrive
{
    public class ExpressionVisitor
    {
        internal Ast.Expression Visit(Ast.Expression expression)
        {
            switch (expression.CodeType)
            {
                case CodeType.BlockExpression:
                    return VisitBlockExpression((Ast.BlockExpression)expression);
                case CodeType.TypeExpression:
                    return VisitTypeExpression((Ast.TypeExpression)expression);
                case CodeType.LambdaExpresion:
                    return VisitLambdaExpression((Ast.LambdaExpression)expression);
                case CodeType.LogicalExpression:
                    return VisitLogicalExpression((Ast.LogicalExpression)expression);
                case CodeType.BinaryExpression:
                    return VisitBinaryExpression((Ast.BinaryExpression)expression);
                case CodeType.LiteralExpression:
                    return VisitLiteralExpression((Ast.LiteralExpression)expression);
                case CodeType.MemberExpression:
                    return VisitMemberExpression((Ast.MemberExpression)expression);
                case CodeType.OrderbyExpression:
                    return VisitOrderbyExpression((Ast.OrderbyExpression)expression);
            }

            throw new ArgumentException("Expression type is not supported");
        }

        public virtual Ast.Expression VisitTypeExpression(Ast.TypeExpression typeExpression)
        {
            return typeExpression;
        }

        public virtual Ast.Expression VisitBlockExpression(Ast.BlockExpression blockExpression)
        {
            foreach (var expression in blockExpression.Expressions)
                this.Visit(expression);

            return blockExpression;
        }

        public virtual Ast.Expression VisitLogicalExpression(Ast.LogicalExpression expression)
        {
            this.Visit(expression.Left);
            this.Visit(expression.Right);
            return expression;
        }

        public virtual Ast.Expression VisitLambdaExpression(Ast.LambdaExpression expression)
        {
            if (expression.Body != null)
                return this.Visit(expression.Body);
            return expression;
        }

        public virtual Ast.Expression VisitBinaryExpression(Ast.BinaryExpression expression)
        {
            this.Visit(expression.Left);
            this.Visit(expression.Right);

            return expression;
        }

        public virtual Ast.Expression VisitMemberExpression(Ast.MemberExpression expression)
        {
            return expression;
        }

        public virtual Ast.Expression VisitLiteralExpression(Ast.LiteralExpression expression)
        {
            return expression;
        }

        public virtual Ast.Expression VisitOrderbyExpression(Ast.OrderbyExpression expression)
        {
            return expression;
        }
  
    }
}
