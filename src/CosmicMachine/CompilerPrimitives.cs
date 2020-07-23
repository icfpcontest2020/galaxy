using System;
using System.Collections.Generic;
using System.Linq;

using Core;

using CosmicMachine.CSharpGalaxy;
using CosmicMachine.Lang;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CosmicMachine
{
    public class CompilerPrimitives
    {
        public static Exp CompileExpression(ExpressionSyntax expression, CompilationContext context)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
                    return RegisterAndUseImage((string)literal.Token.Value!, context);
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NumericLiteralExpression):
                    return literal.Token.Value! is int ? new Num((int)literal.Token.Value!) : new Num((long)literal.Token.Value!);
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.TrueLiteralExpression):
                    return CoreImplementations.True;
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.FalseLiteralExpression):
                    return CoreImplementations.False;
                case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NullLiteralExpression):
                    return CoreImplementations.emptyList;
                case SimpleNameSyntax id:
                    return GetFunction(id, context);
                case BinaryExpressionSyntax bin when bin.IsKind(SyntaxKind.EqualsExpression) && bin.Right.IsKind(SyntaxKind.NullLiteralExpression):
                    return CoreImplementations.isEmptyList.Call(CompileExpression(bin.Left, context));
                case BinaryExpressionSyntax bin when bin.IsKind(SyntaxKind.NotEqualsExpression) && bin.Right.IsKind(SyntaxKind.NullLiteralExpression):
                    return CoreImplementations.not.Call(CoreImplementations.isEmptyList.Call(CompileExpression(bin.Left, context)));
                case BinaryExpressionSyntax bin:
                    {
                        var left = CompileExpression(bin.Left, context);
                        var right = CompileExpression(bin.Right, context);
                        if (bin.IsKind(SyntaxKind.AddExpression))
                            return left + right;
                        if (bin.IsKind(SyntaxKind.MultiplyExpression))
                            return left * right;
                        if (bin.IsKind(SyntaxKind.SubtractExpression))
                            return -right + left;
                        if (bin.IsKind(SyntaxKind.DivideExpression))
                            return left / right;
                        if (bin.IsKind(SyntaxKind.ModuloExpression))
                            return left % right;
                        if (bin.IsKind(SyntaxKind.LessThanExpression))
                            return left < right;
                        if (bin.IsKind(SyntaxKind.GreaterThanExpression))
                            return left > right;
                        if (bin.IsKind(SyntaxKind.LessThanOrEqualExpression))
                            return left <= right;
                        if (bin.IsKind(SyntaxKind.GreaterThanOrEqualExpression))
                            return left >= right;
                        if (bin.IsKind(SyntaxKind.EqualsExpression))
                            return right == left;
                        if (bin.IsKind(SyntaxKind.NotEqualsExpression))
                            return left != right;
                        if (bin.IsKind(SyntaxKind.LogicalAndExpression))
                            return left & right;
                        if (bin.IsKind(SyntaxKind.LogicalOrExpression))
                            return left | right;
                        break;
                    }
                case PrefixUnaryExpressionSyntax prefixUnary:
                    {
                        var val = CompileExpression(prefixUnary.Operand, context);
                        if (prefixUnary.IsKind(SyntaxKind.UnaryMinusExpression))
                            return CoreImplementations.negate.Call(val);
                        if (prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
                            return CoreImplementations.not.Call(val);
                        break;
                    }
                case ParenthesizedExpressionSyntax paren:
                    return CompileExpression(paren.Expression, context);
                case ConditionalExpressionSyntax cond:
                    return CoreImplementations.If(
                        CompileExpression(cond.Condition, context),
                        CompileExpression(cond.WhenTrue, context),
                        CompileExpression(cond.WhenFalse, context));
                case InvocationExpressionSyntax inv:
                    {
                        var argsSyn = inv.ArgumentList.Arguments.Select(a => a.Expression).ToArray();
                        if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            // call as method (smth.F(...))
                            var isExtensionMethod = !(context.SemanticModel.GetSymbolInfo(memberAccess.Expression).Symbol is ITypeSymbol);
                            var arguments = isExtensionMethod ? argsSyn.Prepend(memberAccess.Expression).ToArray() : argsSyn;
                            var funcName = memberAccess.Name.Identifier.Text;
                            var res = TryCompileSpecialFunctions(funcName, arguments, context);
                            if (!(res is null))
                                return res;
                            // Call as static method Core.Pair(a, b)
                            var func = GetFunction(memberAccess, context);
                            return CreateFunCall(func, arguments, context);
                        }
                        // Call as simple function f(x)
                        else if (inv.Expression is SimpleNameSyntax nameSyntax)
                        {
                            var res = TryCompileSpecialFunctions(nameSyntax.Identifier.Text, argsSyn, context);
                            if (!(res is null))
                                return res;
                        }
                        return CreateFunCall(CompileExpression(inv.Expression, context), argsSyn, context);
                    }
                case SimpleLambdaExpressionSyntax simpleLambda:
                    {
                        var body =
                            simpleLambda.ExpressionBody != null
                                ? CompileExpression(simpleLambda.ExpressionBody!, context)
                                : CompileBlock(simpleLambda.Block!.Statements, 0, context);
                        return Exp.Lambda(simpleLambda.Parameter.Identifier.Text, body);
                    }
                case ParenthesizedLambdaExpressionSyntax parenLambda:
                    {
                        var body =
                            parenLambda.ExpressionBody != null
                                ? CompileExpression(parenLambda.ExpressionBody!, context)
                                : CompileBlock(parenLambda.Block!.Statements, 0, context);
                        var vars = parenLambda.ParameterList.Parameters.Select(p => p.Identifier.Text);
                        return Exp.Lambda(vars, body);
                    }
                case CastExpressionSyntax cast:
                    return CompileExpression(cast.Expression, context);
                case ImplicitArrayCreationExpressionSyntax impArray:
                    var values = impArray.Initializer.Expressions.Reverse().Select(expression1 => CompileExpression(expression1, context));
                    return values.Aggregate(CoreImplementations.emptyList, (res, v) => new Pair(v, res));
                case ArrayCreationExpressionSyntax array:
                    return array.Initializer!
                           .Expressions
                           .Reverse()
                           .Aggregate(
                               CoreImplementations.emptyList,
                               (res, v) => new Pair(CompileExpression(v, context), res));
                case MemberAccessExpressionSyntax memberAccess:
                    return CompileMemberAccess(memberAccess, context);
                case TupleExpressionSyntax tuple:
                    return CoreImplementations.Tuple(tuple.Arguments.Select(a => CompileExpression(a.Expression, context)).ToArray());
                case ObjectCreationExpressionSyntax objCreation:
                    return CoreImplementations.List(objCreation.ArgumentList!.Arguments.Select(a => CompileExpression(a.Expression, context)).ToArray());
            }
            throw new NotSupportedException($"{expression}: {expression.GetType()} {expression.Kind()}");
        }

        private static Exp RegisterAndUseImage(string symbolName, CompilationContext context)
        {
            return CoreImplementations.BitEncodeSymbolByName(symbolName);
        }

        public static Exp CompileMemberAccess(MemberAccessExpressionSyntax memberAccess, CompilationContext context)
        {
            var name = memberAccess.Name.Identifier.Text;
            if (name == "Item1")
                return CompileExpression(memberAccess.Expression, context).First();
            if (name == "Item2")
                return CompileExpression(memberAccess.Expression, context).Second();
            var constantValue = context.SemanticModel.GetConstantValue(memberAccess);
            if (constantValue.HasValue)
                return (long)(int)constantValue.Value;

            return GetFunction(memberAccess, context);
        }

        public static Exp CompileMethod(MethodDeclarationSyntax method, CompilationContext context)
        {
            var body = CompileBody(method, context);
            var vars = method.ParameterList.Parameters.Select(p => p.Identifier.Text);
            return Exp.Lambda(vars, body);
        }

        private static Exp CompileBody(MethodDeclarationSyntax method, CompilationContext context)
        {
            if (method.Body != null)
                return CompileBlock(method.Body.Statements, 0, context);
            if (method.ExpressionBody != null)
                return CompileExpression(method.ExpressionBody.Expression, context);
            throw new NotSupportedException($"{method}");
        }

        private static Exp CompileBlock(in SyntaxList<StatementSyntax> statements, int startIndex, CompilationContext context)
        {
            var statement = statements[startIndex];
            if (startIndex == statements.Count - 1)
                return CompileStatement(statement, context);
            switch (statement)
            {
                case LocalDeclarationStatementSyntax localDecl
                    when localDecl.Declaration.Variables.Count == 1
                         && localDecl.Declaration.Variables[0].Initializer != null:
                    var localVar = localDecl.Declaration.Variables[0];
                    return new FunCall(
                        new Lambda(
                            localVar.Identifier.Text,
                            CompileBlock(statements, startIndex + 1, context)),
                        CompileExpression(localVar.Initializer!.Value, context).Log(localVar.Identifier.Text, context));
                case ExpressionStatementSyntax exp
                    when exp.Expression is AssignmentExpressionSyntax assignment
                        && assignment.Left is MemberAccessExpressionSyntax memberAccess
                        && memberAccess.Expression is IdentifierNameSyntax idName:
                    {
                        ISymbol symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol!;
                        var methods = symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().ToList();
                        var ctor = methods[0];
                        var index = ctor.Parameters.IndexOf(p => p.Name.Equals(symbol.Name, StringComparison.InvariantCultureIgnoreCase));
                        if (index >= 0)
                        {
                            Exp setByIndex = CompileKnownFunction("CollectionsModule.SetByIndex", context);
                            context.AddDependency("CollectionsModule.SetByIndex");
                            var varName = idName.Identifier.Text;
                            var newValue = Exp.Call(setByIndex, new Var(varName), index, CompileExpression(assignment.Right, context));
                            return
                                new FunCall(
                                    Exp.Lambda(varName,
                                        CompileBlock(statements, startIndex + 1, context).Log(varName, context)),
                                    newValue);
                        }
                        throw new NotSupportedException($"{statement}");
                    }
                case ExpressionStatementSyntax exp
                    when exp.Expression is AssignmentExpressionSyntax ass
                         && ass.Left is DeclarationExpressionSyntax decl
                         && decl.Designation is ParenthesizedVariableDesignationSyntax parenVarDes
                         && parenVarDes.Variables.All(v => v is SingleVariableDesignationSyntax || v is DiscardDesignationSyntax):

                    var varNames = parenVarDes.Variables.Select(GetArgumentName).ToList();
                    var rightExpr = CompileExpression(ass.Right, context);
                    return rightExpr.DeconstructList(varNames, CompileBlock(statements, startIndex + 1, context).Log(varNames.StrJoin("_"), context));
                case IfStatementSyntax ifStatement when ifStatement.Else == null:
                    return CoreImplementations.If(
                        CompileExpression(ifStatement.Condition, context),
                        CompileStatement(ifStatement.Statement, context),
                        CompileBlock(statements, startIndex + 1, context));
                case LocalFunctionStatementSyntax localFunc:
                    {
                        var body = CompileBlock(localFunc.Body!.Statements, 0, context);
                        var parameters = localFunc.ParameterList.Parameters.Select(p => p.Identifier.Text);
                        var func = Exp.Lambda(parameters, body);
                        return Exp.Call(
                            new Lambda(localFunc.Identifier.Text, CompileBlock(statements, startIndex + 1, context)),
                            func);
                    }
            }
            throw new NotSupportedException($"{statement} {statement.GetType()} {statement.Kind()}");
        }

        private static string GetArgumentName(VariableDesignationSyntax varDesignation)
        {
            if (varDesignation is SingleVariableDesignationSyntax v)
                return v.Identifier.Text;
            if (varDesignation is DiscardDesignationSyntax)
                return "_";
            throw new NotSupportedException(varDesignation.ToString());
        }

        private static Exp CompileStatement(StatementSyntax statement, CompilationContext context)
        {
            switch (statement)
            {
                case BlockSyntax block:
                    return CompileBlock(block.Statements, 0, context);
                case ReturnStatementSyntax returnStatement:
                    return CompileExpression(returnStatement.Expression ?? throw new NotSupportedException($"{statement}"), context);
                case IfStatementSyntax ifStatement when ifStatement.Else != null:
                    return CoreImplementations.If(
                        CompileExpression(ifStatement.Condition, context),
                        CompileStatement(ifStatement.Statement, context),
                        CompileStatement(ifStatement.Else.Statement, context));
                default:
                    throw new NotSupportedException($"{statement}");
            }
        }

        private static Exp GetFunction(CSharpSyntaxNode syntaxNode, CompilationContext context)
        {
            ISymbol symbol = context.SemanticModel.GetSymbolInfo(syntaxNode).Symbol!;
            if (symbol is ILocalSymbol || symbol is IParameterSymbol || symbol is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.LocalFunction)
                return new Var(symbol.Name);
            var name = symbol.ContainingType.Name + "." + symbol.Name;
            if (context.KnownFunctions.TryGetValue(name, out var func))
            {
                if (func.BuiltIn)
                    return func.Body;
                context.AddDependency(name);
                return CompileKnownFunction(name, context);
            }
            if (name == "CoreHeaders.As")
                return CoreImplementations.I;
            // Access to data structure field
            if (syntaxNode is MemberAccessExpressionSyntax memberAccess)
            {
                var methods = symbol.ContainingType.GetMembers().OfType<IMethodSymbol>().Where(s => s.MethodKind == MethodKind.Constructor).ToList();
                var ctor = methods[0];
                var index = ctor.Parameters.IndexOf(p => p.Name.Equals(symbol.Name, StringComparison.InvariantCultureIgnoreCase));
                if (index >= 0)
                {
                    var getByIndex = GetFunctionByName("CollectionsModule.GetByIndex", context);
                    return Exp.Call(getByIndex, CompileExpression(memberAccess.Expression, context), index);
                }
            }
            throw new InvalidOperationException(name + " in " + syntaxNode.Parent?.ToFullString() ?? syntaxNode.ToFullString());
        }

        private static Exp GetFunctionByName(string funName, CompilationContext context)
        {
            Exp getByIndex = CompileKnownFunction(funName, context);
            context.AddDependency(funName);
            return getByIndex;
        }

        private static KnownFunction CompileKnownFunction(string name, CompilationContext context)
        {
            return new KnownFunction(new Lazy<SkiFunction>(() => context.KnownFunctions[name]));
        }

        private static Exp CreateFunCall(Exp func, IEnumerable<ExpressionSyntax> args, CompilationContext context)
        {
            return func.Call(args.Select(a => CompileExpression(a, context)).ToArray());
        }

        private static Exp? TryCompileSpecialFunctions(string funcName, ExpressionSyntax[] args, CompilationContext context)
        {
            if (funcName == nameof(CoreHeaders.List))
                return CoreImplementations.List(args.Select(expression => CompileExpression(expression, context)).ToArray());
            if (funcName == nameof(CSharpGalaxy.CoreHeaders.Tuple))
                return CoreImplementations.Tuple(args.Select(expression => CompileExpression(expression, context)).ToArray());
            if (funcName == nameof(CoreHeaders.LogWithLabel))
            {
                if (context.WithDebugInfo)
                {
                    var message = (string)(((LiteralExpressionSyntax)args[1]).Token.Value!);
                    return CoreImplementations.Log(message, CompileExpression(args[0], context));
                }
                else
                    return CompileExpression(args[0], context);
            }
            if (funcName == nameof(CoreHeaders.DrawSymbolByName))
                return CoreImplementations.PrintSymbolByName(StringLiteral(args.Single()));
            if (funcName == nameof(CoreHeaders.DrawText))
                return CoreImplementations.PrintText(StringLiteral(args.Single()), GetFunctionByName("ComputerModule.PrintSymbol", context), GetFunctionByName("ComputerModule.PrintGlyphs", context));
            if (funcName == nameof(CoreHeaders.DrawImage))
                return CoreImplementations.PrintImage(StringLiteral(args.Single()));
            if (funcName == nameof(CoreHeaders.DrawBitmap))
                return CoreImplementations.PrintBitmap(StringLiteral(args.Single()));
            if (funcName == nameof(CoreHeaders.BitEncodeBitmap))
                return CoreImplementations.BitEncodeBitmap(StringLiteral(args.Single()));
            if (funcName == nameof(CoreHeaders.EncodeBitmap))
                return CoreImplementations.EncodeBitmap(LongLiteral(args[0]), LongLiteral(args[1]), StringLiteral(args[2]));
            if (funcName == nameof(CoreHeaders.BitEncodeImage))
                return CoreImplementations.BitEncodeImage(StringLiteral(args.Single()));
            if (funcName == nameof(CoreHeaders.BitEncodeSymbol))
                return CoreImplementations.BitEncodeSymbol(StringLiteral(args.Single()));
            return null;
        }

        private static string StringLiteral(ExpressionSyntax arg)
        {
            return (string)((LiteralExpressionSyntax)arg).Token.Value!;
        }

        private static long LongLiteral(ExpressionSyntax arg)
        {
            if (arg is PrefixUnaryExpressionSyntax prefix)
                if (prefix.IsKind(SyntaxKind.UnaryMinusExpression))
                    return -LongLiteral(prefix.Operand);
            var value = ((LiteralExpressionSyntax)arg).Token.Value!;
            if (value is long longValue) return longValue;
            if (value is int intValue) return intValue;
            throw new Exception($"int or long expected, but {value.GetType()} found");
        }
    }

    public static class CompilerExtensions
    {
        public static Exp Log(this Exp exp, string label, CompilationContext context)
        {
            if (!context.WithDebugInfo)
                return exp;
            var funName = context.CurrentCompilingFunction;
            var standardFunction = new[] { nameof(CollectionsModule), nameof(ComputerModule), nameof(UiModule) }.Any(m => funName.StartsWith(m));
            if (standardFunction)
                return exp;
            return CoreImplementations.Log(context.CurrentCompilingFunction + "__" + label, exp);
        }
    }
}