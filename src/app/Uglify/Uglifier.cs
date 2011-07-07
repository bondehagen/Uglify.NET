using System;
using System.Collections.Generic;
using System.IO;

using IronJS;
using IronJS.Compiler;
using IronJS.Hosting;
using IronJS.Support;
using Uglify.CommonUtils;

namespace Uglify
{
   /// <summary>
   /// The main Uglify object.
   /// </summary>
   public class Uglifier
   {
      private readonly CSharp.Context context;
      private readonly ResourceHelper resourceHelper;
      private readonly FunctionObject uglify;


      /// <summary>
      /// Initializes a new instance of the <see cref="Uglifier"/> class.
      /// </summary>
      public Uglifier()
      {
         this.resourceHelper = new ResourceHelper();
         this.context = SetupContext(this.resourceHelper);
         this.uglify = LoadUglify(this.context, this.resourceHelper);
      }


      /// <summary>
      /// Uglifies the specified code.
      /// </summary>
      /// <param name="code">The JavaScript code that is to be uglified.</param>
      /// <param name="options">The options.</param>
      /// <returns>
      /// The uglified code.
      /// </returns>
      public string Uglify(
         string code,
         // TODO: The options can probably be made into a neat little object, figure out how. [asbjornu]
         string options = "")
      {
         if (code == null)
            throw new ArgumentNullException("code");

         try
         {
            AstTranslator astTranslator = new AstTranslator();
            StringWriter writer = new StringWriter();
            Action<object> action = x => writer.WriteLine(astTranslator.ToParseJs((Ast.Tree.Function)x));
            Helper.registerAstPrinter(action);
            this.context.Execute(code);
            Helper.removeAstPrinter(action);
            string ast = writer.ToString();
            CommonObject test = this.context.Execute(ast);
            BoxedValue result = this.context.GetGlobalAs<FunctionObject>("uglify2")
               .Call(this.context.Globals, test, "");

            // BoxedValue result = this.uglify.Call(this.context.Globals, code, options);
            return TypeConverter.ToString(result);
         }
         catch (UserError error)
         {
            throw new UglifyException(code, error);
         }
      }


      private static FunctionObject LoadUglify(CSharp.Context context, ResourceHelper resourceHelper)
      {
         string uglifyCode = resourceHelper.Get("uglify-js.js");
         context.Execute(String.Concat(uglifyCode));

         return context.GetGlobalAs<FunctionObject>("uglify");
      }


      private static CSharp.Context SetupContext(ResourceHelper resourceHelper)
      {
         var context = new CSharp.Context();

         //context.CreatePrintFunction();
         // Debug.registerConsolePrinter();
         // IronJS.Support.Debug.registerAstPrinter(AstPrinter);
         // IronJS.Support.Debug.registerExprPrinter(ExprPrinter);
         
         ConsoleConstructor.AttachToContext(context);
          var require = new RequireDefinition(context, resourceHelper);
         require.Define();

         return context;
      }




      private static void AstPrinter(string value)
      {
         Console.Write(value);
      }


      private static void ExprPrinter(string value)
      {
         Console.Write(value);
      }
   }
}