﻿using Antlr4.Runtime.Misc;
using System.Collections.Generic;
using static Compiler.XsParser;

namespace Compiler
{
    internal partial class Visitor
    {
        public override object VisitVariableStatement(VariableStatementContext context)
        {
            var obj = "";
            var r1 = (Result)Visit(context.expression(0));
            var r2 = (Result)Visit(context.expression(1));
            if (context.type() != null)
            {
                var Type = (string)Visit(context.type());
                obj = $"{Type} {r1.text} = {r2.text} {Terminate} {Wrap}";
            }
            else
            {
                obj = $"var {r1.text} = {r2.text} {Terminate} {Wrap}";
            }
            return obj;
        }

        public override object VisitVariableDeclaredStatement([NotNull] VariableDeclaredStatementContext context)
        {
            var obj = "";
            var Type = (string)Visit(context.type());
            var r = (Result)Visit(context.expression());
            obj = $"{Type} {r.text} {Terminate} {Wrap}";
            return obj;
        }

        public override object VisitAssignStatement(AssignStatementContext context)
        {
            var r1 = (Result)Visit(context.expression(0));
            var r2 = (Result)Visit(context.expression(1));
            var obj = r1.text + Visit(context.assign()) + r2.text + Terminate + Wrap;
            return obj;
        }

        public override object VisitAssign([NotNull] AssignContext context)
        {
            if (context.op.Type == Assign)
            {
                return "=";
            }
            return context.op.Text;
        }

        public override object VisitExpressionStatement([NotNull] ExpressionStatementContext context)
        {
            var r = (Result)Visit(context.expression());
            return r.text + Terminate + Wrap;
        }

        public override object VisitExpression([NotNull] ExpressionContext context)
        {
            var count = context.ChildCount;
            var r = new Result();
            if (count == 3)
            {
                var e1 = (Result)Visit(context.GetChild(0));
                var op = Visit(context.GetChild(1));
                var e2 = (Result)Visit(context.GetChild(2));
                if (context.GetChild(1).GetType() == typeof(CallContext))
                {
                    r.data = "var";
                    for (int i = 0; i < e2.bracketTime; i++)
                    {
                        r.text += "(";
                    }
                    switch (e2.callType)
                    {
                        case "element":
                            r.text = e1.text + e2.text;
                            return r;
                        case "as":
                        case "is":
                            r.data = e2.data;
                            if (e2.isCall)
                            {
                                r.text += e1.text + e2.text;
                            }
                            else
                            {
                                r.text += e1.text + op + e2.text;
                            }
                            return r;
                        default:
                            break;
                    }
                }
                if (context.GetChild(1).GetType() == typeof(JudgeContext))
                {
                    // todo 如果左右不是bool类型值，报错
                    r.data = bl;
                }
                else if (context.GetChild(1).GetType() == typeof(AddContext))
                {
                    // todo 如果左右不是number或text类型值，报错
                    if ((string)e1.data == str || (string)e2.data == str)
                    {
                        r.data = str;
                    }
                    else if ((string)e1.data == i32 && (string)e2.data == i32)
                    {
                        r.data = i32;
                    }
                    else
                    {
                        r.data = f64;
                    }
                }
                else if (context.GetChild(1).GetType() == typeof(MulContext))
                {
                    // todo 如果左右不是number类型值，报错
                    if ((string)e1.data == i32 && (string)e2.data == i32)
                    {
                        r.data = i32;
                    }
                    else
                    {
                        r.data = i32;
                    }
                }
                r.text = e1.text + op + e2.text;
            }
            else if (count == 1)
            {
                r = (Result)Visit(context.GetChild(0));
            }
            return r;
        }

        public override object VisitCallSelf([NotNull] CallSelfContext context)
        {
            var r = new Result
            {
                data = "var"
            };
            var e1 = "this";
            var op = ".";
            var e2 = (Result)Visit(context.GetChild(1));
            for (int i = 0; i < e2.bracketTime; i++)
            {
                r.text += "(";
            }
            switch (e2.callType)
            {
                case "element":
                    r.text = e1 + e2.text;
                    return r;
                case "as":
                case "is":
                    r.data = e2.data;
                    if (e2.isCall)
                    {
                        r.text += e1 + e2.text;
                    }
                    else
                    {
                        r.text += e1 + op + e2.text;
                    }
                    return r;
                default:
                    break;
            }
            r.text = e1 + op + e2.text;
            return r;
        }

        public override object VisitCallNameSpace([NotNull] CallNameSpaceContext context)
        {
            var obj = "";
            for (int i = 0; i < context.id().Length; i++)
            {
                var id = (Result)Visit(context.id(i));
                if (i == 0)
                {
                    obj += "" + id.text;
                }
                else
                {
                    obj += "." + id.text;
                }
            }

            var r = new Result
            {
                data = "var"
            };
            var e1 = obj;
            var op = ".";
            var e2 = (Result)Visit(context.callExpression());
            for (int i = 0; i < e2.bracketTime; i++)
            {
                r.text += "(";
            }
            switch (e2.callType)
            {
                case "element":
                    r.text = e1 + e2.text;
                    return r;
                case "as":
                case "is":
                    r.data = e2.data;
                    if (e2.isCall)
                    {
                        r.text += e1 + e2.text;
                    }
                    else
                    {
                        r.text += e1 + op + e2.text;
                    }
                    return r;
                default:
                    break;
            }
            r.text = e1 + op + e2.text;
            return r;
        }

        public override object VisitCallExpression([NotNull] CallExpressionContext context)
        {
            var count = context.ChildCount;
            var r = new Result();
            if (count == 3)
            {
                var e1 = (Result)Visit(context.GetChild(0));
                var op = Visit(context.GetChild(1));
                var e2 = (Result)Visit(context.GetChild(2));
                if (context.GetChild(0).GetChild(0) is CallElementContext)
                {
                    r.callType = "element";
                }
                r.isCall = e1.isCall;
                r.callType = e1.callType;
                if (e1.bracketTime > 0)
                {
                    r.bracketTime += e1.bracketTime;
                }
                if (context.GetChild(2).GetChild(0) is CallElementContext)
                {
                    r.text = e1.text + e2.text;
                    return r;
                }
                else if (context.GetChild(2).GetChild(0) is CallAsContext)
                {
                    r.callType = "as";
                    r.data = e2.data;
                    r.text = e1.text + e2.text;
                    r.bracketTime = e1.bracketTime + 1;
                    return r;
                }
                else if (context.GetChild(2).GetChild(0) is CallIsContext)
                {
                    r.callType = "is";
                    r.data = e2.data;
                    r.text = e1.text + e2.text;
                    r.bracketTime = e1.bracketTime + 1;
                    return r;
                }
                r.text = e1.text + op + e2.text;
            }
            else if (count == 1)
            {
                r = (Result)Visit(context.GetChild(0));
                if (context.GetChild(0) is CallElementContext)
                {
                    r.callType = "element";
                }
                else if (context.GetChild(0) is CallAsContext)
                {
                    r.callType = "as";
                    r.bracketTime++;
                    r.isCall = true;
                }
                else if (context.GetChild(0) is CallIsContext)
                {
                    r.callType = "is";
                    r.bracketTime++;
                    r.isCall = true;
                }
            }
            return r;
        }

        public override object VisitCall([NotNull] CallContext context)
        {
            return context.op.Text;
        }

        public override object VisitWave([NotNull] WaveContext context)
        {
            return context.op.Text;
        }

        public override object VisitJudge([NotNull] JudgeContext context)
        {
            if (context.op.Text == "~=")
            {
                return "!=";
            }
            else if (context.op.Text == "&")
            {
                return "&&";
            }
            else if (context.op.Text == "|")
            {
                return "||";
            }
            return context.op.Text;
        }

        public override object VisitAdd([NotNull] AddContext context)
        {
            return context.op.Text;
        }

        public override object VisitMul([NotNull] MulContext context)
        {
            return context.op.Text;
        }

        public override object VisitPrimaryExpression([NotNull] PrimaryExpressionContext context)
        {
            if (context.ChildCount == 1)
            {
                var c = context.GetChild(0);
                if (c is DataStatementContext)
                {
                    return Visit(context.dataStatement());
                }
                else if (c is IdContext)
                {
                    return Visit(context.id());
                }
                else if (context.t.Type == Self)
                {
                    return new Result { text = "this", data = "var" };
                }
                else if (context.t.Type == Discard)
                {
                    return new Result { text = "_", data = "var" };
                }
            }
            var r = (Result)Visit(context.expression());
            return new Result { text = "(" + r.text + ")", data = r.data };
        }

        public override object VisitExpressionList([NotNull] ExpressionListContext context)
        {
            var r = new Result();
            var obj = "";
            for (int i = 0; i < context.expression().Length; i++)
            {
                var temp = (Result)Visit(context.expression(i));
                if (i == 0)
                {
                    obj += temp.text;
                }
                else
                {
                    obj += ", " + temp.text;
                }
            }
            r.text = obj;
            r.data = "var";
            return r;
        }

        public override object VisitTemplateDefine([NotNull] TemplateDefineContext context)
        {
            var obj = "";
            obj += "<";
            for (int i = 0; i < context.id().Length; i++)
            {
                if (i > 0)
                {
                    obj += ",";
                }
                var r = (Result)Visit(context.id(i));
                obj += r.text;
            }
            obj += ">";
            return obj;
        }

        public override object VisitTemplateCall([NotNull] TemplateCallContext context)
        {
            var obj = "";
            obj += "<";
            for (int i = 0; i < context.type().Length; i++)
            {
                if (i > 0)
                {
                    obj += ",";
                }
                var r = Visit(context.type(i));
                obj += r;
            }
            obj += ">";
            return obj;
        }

        public override object VisitCallElement([NotNull] CallElementContext context)
        {
            if (context.expression() == null)
            {
                return new Result { text = (string)Visit(context.slice()) };
            }
            var r = (Result)Visit(context.expression());
            r.text = "[" + r.text + "]";
            return r;
        }

        public override object VisitSlice([NotNull] SliceContext context)
        {
            return (string)Visit(context.GetChild(0));
        }

        public override object VisitSliceFull([NotNull] SliceFullContext context)
        {
            var order = "";
            var attach = "";
            switch (context.op.Text)
            {
                case "<=":
                    order = "true";
                    attach = "true";
                    break;
                case "<":
                    order = "true";
                    break;
                case ">=":
                    order = "false";
                    attach = "true";
                    break;
                case ">":
                    order = "false";
                    break;
                default:
                    break;
            }
            var expr1 = (Result)Visit(context.expression(0));
            var expr2 = (Result)Visit(context.expression(1));
            return $".slice({expr1.text}, {expr2.text}, {order}, {attach})";
        }

        public override object VisitSliceStart([NotNull] SliceStartContext context)
        {
            var order = "";
            var attach = "";
            switch (context.op.Text)
            {
                case "<=":
                    order = "true";
                    attach = "true";
                    break;
                case "<":
                    order = "true";
                    break;
                case ">=":
                    order = "false";
                    attach = "true";
                    break;
                case ">":
                    order = "false";
                    break;
                default:
                    break;
            }
            var expr = (Result)Visit(context.expression());
            return $".slice({expr.text}, null, {order}, {attach})";
        }

        public override object VisitSliceEnd([NotNull] SliceEndContext context)
        {
            var order = "";
            var attach = "false";
            switch (context.op.Text)
            {
                case "<=":
                    order = "true";
                    attach = "true";
                    break;
                case "<":
                    order = "true";
                    break;
                case ">=":
                    order = "false";
                    attach = "true";
                    break;
                case ">":
                    order = "false";
                    break;
                default:
                    break;
            }
            var expr = (Result)Visit(context.expression());
            return $".slice(null, {expr.text}, {order}, {attach})";
        }

        public override object VisitCallFunc([NotNull] CallFuncContext context)
        {
            var r = new Result
            {
                data = "var"
            };
            var id = (Result)Visit(context.id());
            r.text += id.text;
            if (context.templateCall() != null)
            {
                r.text += Visit(context.templateCall());
            }
            r.text += ((Result)Visit(context.tuple())).text;
            return r;
        }

        public override object VisitCallPkg([NotNull] CallPkgContext context)
        {
            var r = new Result
            {
                data = Visit(context.type())
            };
            var param = "";
            if (context.expressionList() != null)
            {
                param = ((Result)Visit(context.expressionList())).text;
            }
            r.text = $"(new {Visit(context.type())}({param})";
            if (context.pkgAssign() != null)
            {
                r.text += Visit(context.pkgAssign());
            }
            if (context.listAssign() != null)
            {
                r.text += Visit(context.listAssign());
            }
            if (context.dictionaryAssign() != null)
            {
                r.text += Visit(context.dictionaryAssign());
            }
            r.text += ")";
            return r;
        }

        public override object VisitPkgAssign([NotNull] PkgAssignContext context)
        {
            var obj = "";
            obj += "{";
            for (int i = 0; i < context.pkgAssignElement().Length; i++)
            {
                if (i == 0)
                {
                    obj += Visit(context.pkgAssignElement(i));
                }
                else
                {
                    obj += "," + Visit(context.pkgAssignElement(i));
                }
            }
            obj += "}";
            return obj;
        }

        public override object VisitListAssign([NotNull] ListAssignContext context)
        {
            var obj = "";
            obj += "{";
            for (int i = 0; i < context.expression().Length; i++)
            {
                var r = (Result)Visit(context.expression(i));
                if (i == 0)
                {
                    obj += r.text;
                }
                else
                {
                    obj += "," + r.text;
                }
            }
            obj += "}";
            return obj;
        }

        public override object VisitDictionaryAssign([NotNull] DictionaryAssignContext context)
        {
            var obj = "";
            obj += "{";
            for (int i = 0; i < context.dictionaryElement().Length; i++)
            {
                var r = (DicEle)Visit(context.dictionaryElement(i));
                if (i == 0)
                {
                    obj += r.text;
                }
                else
                {
                    obj += "," + r.text;
                }
            }
            obj += "}";
            return obj;
        }

        public override object VisitPkgAssignElement([NotNull] PkgAssignElementContext context)
        {
            var obj = "";
            obj += Visit(context.name()) + " = " + ((Result)Visit(context.expression())).text;
            return obj;
        }

        public override object VisitPkgAnonymous([NotNull] PkgAnonymousContext context)
        {
            var r = new Result
            {
                data = "var",
                text = "new" + (string)Visit(context.pkgAnonymousAssign())
            };
            return r;
        }

        public override object VisitPkgAnonymousAssign([NotNull] PkgAnonymousAssignContext context)
        {
            var obj = "";
            obj += "{";
            for (int i = 0; i < context.pkgAnonymousAssignElement().Length; i++)
            {
                if (i == 0)
                {
                    obj += Visit(context.pkgAnonymousAssignElement(i));
                }
                else
                {
                    obj += "," + Visit(context.pkgAnonymousAssignElement(i));
                }
            }
            obj += "}";
            return obj;
        }

        public override object VisitPkgAnonymousAssignElement([NotNull] PkgAnonymousAssignElementContext context)
        {
            var obj = "";
            obj += Visit(context.name()) + " = " + ((Result)Visit(context.expression())).text;
            return obj;
        }

        public override object VisitCallAwait([NotNull] CallAwaitContext context)
        {
            var r = new Result();
            var expr = (Result)Visit(context.expression());
            r.data = "var";
            r.text = "await " + expr.text;
            return r;
        }

        public override object VisitArray([NotNull] ArrayContext context)
        {
            var type = "var";
            var result = new Result();
            for (int i = 0; i < context.expression().Length; i++)
            {
                var r = (Result)Visit(context.expression(i));
                if (i == 0)
                {
                    type = (string)r.data;
                    result.text += r.text;
                }
                else
                {
                    if (type != (string)r.data)
                    {
                        type = "object";
                    }
                    result.text += "," + r.text;
                }
            }
            if (context.type() != null)
            {
                result.data = $"{(string)Visit(context.type())}[]";
            }
            else
            {
                result.data = type + "[]";
            }

            result.text = $"(new {result.data}{{ {result.text} }})";
            return result;
        }

        public override object VisitList([NotNull] ListContext context)
        {
            var type = "object";
            var result = new Result();
            for (int i = 0; i < context.expression().Length; i++)
            {
                var r = (Result)Visit(context.expression(i));
                if (i == 0)
                {
                    type = (string)r.data;
                    result.text += r.text;
                }
                else
                {
                    if (type != (string)r.data)
                    {
                        type = "object";
                    }
                    result.text += "," + r.text;
                }
            }
            if (context.type() != null)
            {
                result.data = $"{lst}<{(string)Visit(context.type())}>";
            }
            else
            {
                result.data = $"{lst}<{type}>";
            }

            result.text = $"(new {result.data}(){{ {result.text} }})";
            return result;
        }

        public override object VisitDictionary([NotNull] DictionaryContext context)
        {
            var key = Any;
            var value = Any;
            var result = new Result();
            for (int i = 0; i < context.dictionaryElement().Length; i++)
            {
                var r = (DicEle)Visit(context.dictionaryElement(i));
                if (i == 0)
                {
                    key = r.key;
                    value = r.value;
                    result.text += r.text;
                }
                else
                {
                    if (key != r.key)
                    {
                        key = Any;
                    }
                    if (value != r.value)
                    {
                        value = Any;
                    }
                    result.text += "," + r.text;
                }
            }
            var type = key + "," + value;
            if (context.type().Length > 0)
            {
                result.data = $"{dic}<{(string)Visit(context.type(0))},{(string)Visit(context.type(1))}>";
            }
            else
            {
                result.data = $"{dic}<{type}>";
            }

            result.text = $"(new {result.data}(){{ {result.text} }})";
            return result;
        }

        private class DicEle
        {
            public string key;
            public string value;
            public string text;
        }

        public override object VisitDictionaryElement([NotNull] DictionaryElementContext context)
        {
            var r1 = (Result)Visit(context.expression(0));
            var r2 = (Result)Visit(context.expression(1));
            var result = new DicEle
            {
                key = (string)r1.data,
                value = (string)r2.data,
                text = "{" + r1.text + "," + r2.text + "}"
            };
            return result;
        }

        public override object VisitDataStatement([NotNull] DataStatementContext context)
        {
            var r = new Result();
            if (context.t.Type == Float)
            {
                r.data = f64;
                r.text = $"{context.Float().GetText()}";
            }
            else if (context.t.Type == Integer)
            {
                r.data = i32;
                r.text = $"{context.Integer().GetText()}";
            }
            else if (context.t.Type == Text)
            {
                r.data = str;
                var text = context.Text().GetText();
                if (text.Contains("{"))
                {
                    text = "$" + text;
                }

                r.text = text;
            }
            else if (context.t.Type == XsParser.Char)
            {
                r.data = chr;
                r.text = context.Char().GetText();
            }
            else if (context.t.Type == XsParser.True)
            {
                r.data = bl;
                r.text = $"{context.True().GetText()}";
            }
            else if (context.t.Type == XsParser.False)
            {
                r.data = bl;
                r.text = $"{context.False().GetText()}";
            }
            else if (context.t.Type == Null)
            {
                r.data = Any;
                r.text = "null";
            }
            return r;
        }

        public override object VisitFunction([NotNull] FunctionContext context)
        {
            var r = new Result();
            // 异步
            if (context.t.Type == FlowRight)
            {
                r.text += " async ";
            }
            r.text += Visit(context.parameterClauseIn()) + " => " + BlockLeft + Wrap;
            r.text += ProcessFunctionSupport(context.functionSupportStatement());
            r.text += BlockRight + Wrap;
            r.data = "var";
            return r;
        }

        public override object VisitLambda([NotNull] LambdaContext context)
        {
            var r = new Result
            {
                data = "var"
            };
            if (context.lambdaShort() != null)
            {
                r.text += Visit(context.lambdaShort());
                return r;
            }
            // 异步
            if (context.t.Type == FlowRight)
            {
                r.text += "async ";
            }
            r.text += "(";
            if (context.lambdaIn() != null)
            {
                r.text += Visit(context.lambdaIn());
            }
            r.text += ")";
            r.text += "=>";

            if (context.expressionList() != null)
            {
                r.text += ((Result)Visit(context.expressionList())).text;
            }
            else
            {
                r.text += "{" + ProcessFunctionSupport(context.functionSupportStatement()) + "}";
            }

            return r;
        }

        public override object VisitLambdaIn([NotNull] LambdaInContext context)
        {
            var obj = "";
            for (int i = 0; i < context.id().Length; i++)
            {
                var r = (Result)Visit(context.id(i));
                if (i == 0)
                {
                    obj += r.text;
                }
                else
                {
                    obj += ", " + r.text;
                }
            }
            return obj;
        }

        public override object VisitLambdaShort([NotNull] LambdaShortContext context)
        {
            var obj = "(it) => ";
            obj += ((Result)Visit(context.expressionList())).text;
            return obj;
        }

        public override object VisitEmpty([NotNull] EmptyContext context)
        {
            var r = new Result();
            var type = Visit(context.type());
            r.data = type;
            r.text = "default(" + type + ")";
            return r;
        }

        public override object VisitPlusMinus([NotNull] PlusMinusContext context)
        {
            var r = new Result();
            var expr = (Result)Visit(context.expression());
            var op = Visit(context.add());
            r.data = expr.data;
            r.text = op + expr.text;
            return r;
        }

        public override object VisitNegate([NotNull] NegateContext context)
        {
            var r = new Result();
            var expr = (Result)Visit(context.expression());
            r.data = expr.data;
            r.text = "!" + expr.text;
            return r;
        }

        private readonly List<string> keywords = new List<string> {   "abstract", "as", "base", "bool", "break" , "byte", "case" , "catch",
                        "char","checked","class","const","continue","decimal","default","delegate","do","double","else",
                        "enum","event","explicit","extern","false","finally","fixed","float","for","foreach","goto",
                        "if","implicit","in","int","interface","internal","is","lock","long","namespace","new","null",
                        "object","operator","out","override","params","private","protected","public","readonly","ref",
                        "return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch",
                        "this","throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using",
                        "virtual","void","volatile","while"
                };
    }
}