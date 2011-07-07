#region License

// --------------------------------------------------
// Copyright © 2003–2010 OKB. All Rights Reserved.
// 
// This software is proprietary information of OKB.
// USE IS SUBJECT TO LICENSE TERMS.
// --------------------------------------------------

#endregion

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

using IronJS;

namespace Uglify.CommonUtils
{
   public class ObjectDumper
   {
      private readonly int depth;
      private int level;
      private int pos;
      private TextWriter writer;


      private ObjectDumper(int depth)
      {
         this.depth = depth;
      }


      public static void Write(object element)
      {
         Write(element, 0);
      }


      public static void Write(object element, int depth)
      {
         Write(element, depth, Console.Out);
      }


      public static void Write(object element, int depth, TextWriter log)
      {
         ObjectDumper dumper = new ObjectDumper(depth) { writer = log };
         dumper.WriteObject(null, element);
      }


      private void Write(string s)
      {
         if (s == null)
            return;
         this.writer.Write(s);
         this.pos += s.Length;
      }


      private void WriteIndent()
      {
         for (int i = 0; i < this.level; i++)
            this.writer.Write("  ");
      }


      private void WriteLine()
      {
         this.writer.WriteLine();
         this.pos = 0;
      }


      private void WriteObject(string prefix, object element)
      {
         if (element.GetType() == typeof(Descriptor))
         {
            Descriptor descriptor = (Descriptor)element;
            element = descriptor.Value;
         }
         if (element.GetType() == typeof(BoxedValue))
         {
            BoxedValue bv = (BoxedValue)element;
            element = bv.UnboxObject();
         }
         if (element == null || element is ValueType || element is string)
         {
            WriteIndent();
            Write(prefix);
            WriteValue(element);
            WriteLine();
         }
         else
         {
            IEnumerable enumerableElement = element as IEnumerable;
            if (enumerableElement != null)
            {
               foreach (object item in enumerableElement)
               {
                  if (item is IEnumerable && !(item is string))
                  {
                     WriteIndent();
                     Write(prefix);
                     Write("...");
                     WriteLine();
                     if (this.level < this.depth)
                     {
                        this.level++;
                        WriteObject(prefix, item);
                        this.level--;
                     }
                  }
                  else
                  {
                     WriteObject(prefix, item);
                  }
               }
            }
            else
            {
               MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(
                  x =>
                  {
                     if (x.DeclaringType != typeof(CommonObject))
                        return true;
                     switch (x.MemberType.ToString())
                     {
                        case "Method":
                        case "Constructor":
                           return false;
                     }
                     string name = x.Name;
                     switch (name)
                     {
                        case "Dense":
                        case "Members":
                       // case "Prototype":
                           return true;
                     }
                     return false;
                  }).ToArray();
               WriteIndent();
               Write(prefix);
               bool propWritten = false;
               foreach (MemberInfo m in members)
               {
                  FieldInfo f = m as FieldInfo;
                  PropertyInfo p = m as PropertyInfo;
                  if (f == null && p == null)
                     continue;

                  if (propWritten)
                     WriteTab();
                  else
                     propWritten = true;

                  /*if (!element.GetType().IsAssignableFrom(typeof(CommonObject)))
                  {
                     Write(m.Name);
                     Write("=");
                  }*/
                  Type t = f != null ? f.FieldType : p.PropertyType;
                  /*if (t.IsValueType || t == typeof(string))
                     WriteValue(f != null ? f.GetValue(element) : p.GetValue(element, null));
                  else
                  {
                     Write(typeof(IEnumerable).IsAssignableFrom(t) ? "..." : "{ }");
                  }*/
               }
               if (propWritten)
                  WriteLine();

               if (this.level < this.depth)
               {
                  foreach (MemberInfo m in members)
                  {
                     FieldInfo f = m as FieldInfo;
                     PropertyInfo p = m as PropertyInfo;
                     if (f == null && p == null)
                        continue;

                     Type t = f != null ? f.FieldType : p.PropertyType;
                     if ((t.IsValueType || t == typeof(string)))
                        continue;

                     object value = f != null ? f.GetValue(element) : p.GetValue(element, null);
                     if (value == null)
                        continue;

                     this.level++;
                     //if (element.GetType().IsAssignableFrom(typeof(CommonObject)))
                        WriteObject(null, value);
                     /*else
                        WriteObject(m.Name + ": ", value);*/
                     this.level--;
                  }
               }
            }
         }
      }


      private void WriteTab()
      {
         Write("  ");
         while (this.pos % 8 != 0)
            Write(" ");
      }


      private void WriteValue(object o)
      {
         if (o == null)
            Write("null");
         else if (o is DateTime)
            Write(((DateTime)o).ToShortDateString());
         else if (o is string)
            Write(string.Format("'{0}'", o));
         else if (o is ValueType)
            Write(o.ToString());
         else if (o is IEnumerable)
            Write("...");
         else
            Write("{ }");
      }
   }
}