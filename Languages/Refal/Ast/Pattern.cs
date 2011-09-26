using System;
using System.Collections.Generic;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Refal.Runtime;

namespace Refal
{
	/// <summary>
	/// Pattern is a passive expression that may contain free variables
	/// </summary>
	public class Pattern : AstNode
	{
		public IList<AstNode> Terms { get; private set; }

		public bool IsEmpty
		{
			get { return Terms.Count == 0; }
		}

		public Pattern()
		{
			Terms = new List<AstNode>();
		}

		public override void Init(ParsingContext context, ParseTreeNode parseNode)
		{
			base.Init(context, parseNode);
			
			foreach (var node in parseNode.ChildNodes)
			{
				if (node.AstNode is AstNode)
					Terms.Add(node.AstNode as AstNode);
			}
		}

		public override System.Collections.IEnumerable GetChildNodes()
		{
			foreach (var term in Terms)
				yield return term;
		}

		protected override object DoEvaluate(ScriptThread thread)
		{
			return EvaluateTerms(thread);
		}

		private object[] EvaluateTerms(ScriptThread thread)
		{
			// standard prolog
			thread.CurrentNode = this;

			try
			{
				var terms = new List<object>();

				foreach (var term in Terms)
				{
					// in pattern, variables are never read
					var result = term.Evaluate(thread);
					terms.Add(result);
				}

				return terms.ToArray();
			}
			finally
			{
				// standard epilog
				thread.CurrentNode = Parent;
			}
		}
		
		public Runtime.Pattern Instantiate(ScriptThread thread)
		{
			// evaluate pattern and instantiate Runtime.Pattern
			return new Runtime.Pattern(EvaluateTerms(thread));
		}

		public override string ToString()
		{
			return "pattern";
		}
	}
}
