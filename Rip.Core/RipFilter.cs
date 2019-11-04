using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rip.Core
{
    public enum RipFilterCommands
    {
        And,
        Or,
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterEqual,
        LessEqual,
        StartsWith,
        EndsWith,
        Contains
    }

    public abstract class RipFilterOp
    {
        public abstract string ToStringifiedJson();
    }

    public class RipFilterBuilder
    {
        public RipFilterOp Filter { get; set; }     
        
        public string ToStringifiedJson()
        {
            return $"{{\"f\":{{{Filter.ToStringifiedJson()}}}";
        }

        public object ToObject()
        {
            return JsonConvert.DeserializeObject(ToStringifiedJson());
        }
    }

    public class RipFilterParameter : RipFilterOp
    {        
        public string Path { get; set; }        

        public override string ToStringifiedJson()
        {
            return $"\"{Path}\"";
        }
    }

    public class RipFilterValue : RipFilterOp
    {
        public object Value { get; set; }

        public override string ToStringifiedJson()
        {
            return JsonConvert.ToString(Value);
        }
    }

    public class RipFilterCommand : RipFilterOp
    {
        public RipFilterCommands Command { get; set; }
              
        public RipFilterOp Left { get; set; }      
        public RipFilterOp Right { get; set; }

        public override string ToStringifiedJson()
        {
            var sb = new StringBuilder();
            
            switch(Command)
            {
                case RipFilterCommands.And: sb.Append("\"and\""); break;
                case RipFilterCommands.Equal: sb.Append("\"=\""); break;
                case RipFilterCommands.Greater: sb.Append("\">\""); break;
                case RipFilterCommands.GreaterEqual: sb.Append("\">=\""); break;
                case RipFilterCommands.Less: sb.Append("\"<\""); break;
                case RipFilterCommands.LessEqual: sb.Append("\"<=\""); break;
                case RipFilterCommands.NotEqual: sb.Append("\"<>\""); break;
                case RipFilterCommands.Or: sb.Append("\"or\""); break;
                case RipFilterCommands.StartsWith: sb.Append("\"=*\""); break;
                case RipFilterCommands.EndsWith: sb.Append("\"*=\""); break;
                case RipFilterCommands.Contains: sb.Append("\"*=*\""); break;
                default:
                    throw new Exception("Unknown filter command.");
            }

            if (Command == RipFilterCommands.And || Command == RipFilterCommands.Or)
            {
                sb.Append(":[{");
                sb.Append(Left.ToStringifiedJson());
                sb.Append("},{");
                sb.Append(Right.ToStringifiedJson());
                sb.Append("}]");
            }
            else
            {
                sb.Append(":{");
                sb.Append(Left.ToStringifiedJson());
                sb.Append(":");
                sb.Append(Right.ToStringifiedJson());
                sb.Append("}");
            }

            return sb.ToString();

        }
    }
}
