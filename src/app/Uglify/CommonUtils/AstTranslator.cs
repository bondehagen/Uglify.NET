using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronJS.Compiler;
using Microsoft.FSharp.Collections;

namespace Uglify.CommonUtils
{
    public class AstTranslator
    {
        public string ToParseJs(Ast.Tree.Function astFunction)
        {
            //string fSharpOption = astFunction.Item1.Value;
            Ast.FunctionScope functionScope = astFunction.Item2.Value;

            Ast.Tree tree = astFunction.Item3;

            return string.Format("['toplevel',{0}]", Walk(tree, ""));
        }

        private string Walk(Ast.Tree tree, string ret)
        {
            Type type = tree.GetType().UnderlyingSystemType;
            PropertyInfo item = type.GetProperty("Item");
                                                                                                                                                  
            if (tree.IsVar)
            {
                Ast.Tree next = ((Ast.Tree.Var) tree).Item;
                ret += string.Format("'var',[{0}]", Walk(next, ret));
            }
            else if (tree.IsIdentifier)
            {
                string identifier = ((Ast.Tree.Identifier) tree).Item;
                ret += string.Format("'{0}'", identifier);
            }
            else if (tree.IsCompoundAssign)
            {
                Ast.Tree.CompoundAssign compoundAssign = (Ast.Tree.CompoundAssign)tree;
               // ret += string.Format("[{0},{1}]", Walk(compoundAssign.Item1, ret), Walk(compoundAssign.Item2, ret));
                throw new NotImplementedException();
            }
            else if (tree.IsAssign)
            {
                Ast.Tree.Assign assign = (Ast.Tree.Assign) tree;
                ret += string.Format("[{0},{1}]", Walk(assign.Item1, ret), Walk(assign.Item2, ret));
            }
            else if (tree.IsBinary)
            {
                Ast.Tree.Binary binary = (Ast.Tree.Binary) tree;
                var operators = new Dictionary<string, string>
                                    {
                                        //{"add", "+"},
                                        {"Add", "+"}
                                    };
                ret += string.Format("['binary','{0}',{1},{2}]",
                                     operators[binary.Item1.ToString()],
                                     Walk(binary.Item2, ret),
                                     Walk(binary.Item3, ret));
            }
            else if (tree.IsNumber)
            {
                double number = ((Ast.Tree.Number) tree).Item;
                ret += string.Format("['num',{0}]", number);
            }
            else if (tree.IsBlock)
            {
                FSharpList<Ast.Tree> trees = ((Ast.Tree.Block) tree).Item;
                IList<string> arr = new List<string>();
                foreach (Ast.Tree next in trees)
                {
                    arr.Add(Walk(next, ret));
                }
                ret += string.Format("[{0}]", string.Join(",",arr.ToArray()));
            }
            else if (tree.IsArray)
            {
                ret += "array";
                FSharpList<Ast.Tree> trees = ((Ast.Tree.Array) tree).Item;
                foreach (Ast.Tree next in trees)
                {
                    ret += string.Format("[{0}]", Walk(next, ret));
                }
            }
            else
            {
                throw new NotImplementedException("Type: " + type);
            }
            return ret;
        }
        /*tree.IsArray
tree.IsAssign
tree.IsBinary
tree.IsBlock
tree.IsBoolean
tree.IsBreak
tree.IsCatch
tree.IsComma
tree.IsCompoundAssign
tree.IsContinue
tree.IsConvert
tree.IsDirective
tree.IsDlrExpr
tree.IsDoWhile
tree.IsEval
tree.IsFor
tree.IsForIn
tree.IsFunction
tree.IsIdentifier
tree.IsIfElse
tree.IsIndex
tree.IsInvoke
tree.IsLabel
tree.IsLine
tree.IsNew
tree.IsNull
tree.IsNumber
tree.IsObject
tree.IsPass
tree.IsProperty
tree.IsRegex
tree.IsReturn
tree.IsString
tree.IsSwitch
tree.IsTernary
tree.IsThis
tree.IsThrow
tree.IsTry
tree.IsUnary
tree.IsUndefined
tree.IsVar
tree.IsWhile
tree.IsWith*/
    }
}