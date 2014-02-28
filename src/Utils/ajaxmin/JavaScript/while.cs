// while.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Kiss.Web.Utils.ajaxmin
{
    public sealed class WhileNode : IterationStatement
    {
        private AstNode m_condition;

        public AstNode Condition
        {
            get { return m_condition; }
            set
            {
                m_condition.IfNotNull(n => n.Parent = (n.Parent == this) ? null : n.Parent);
                m_condition = value;
                m_condition.IfNotNull(n => n.Parent = this);
            }
        }

        public override Context TerminatingContext
        {
            get
            {
                // if we have one, return it. If not, return what the body has (if any)
                return base.TerminatingContext ?? Body.IfNotNull(b => b.TerminatingContext);
            }
        }

        public WhileNode(Context context, JSParser parser)
            : base(context, parser)
        {
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Condition, Body);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Condition == oldNode)
            {
                Condition = newNode;
                return true;
            }
            if (Body == oldNode)
            {
                Body = ForceToBlock(newNode);
                return true;
            }
            return false;
        }

        internal override bool RequiresSeparator
        {
            get
            {
                // requires a separator if the body does
                return Body == null || Body.Count == 0 ? false : Body.RequiresSeparator;
            }
        }

        internal override bool EncloseBlock(EncloseBlockType type)
        {
            // pass the query on to the body
            return Body == null ? false : Body.EncloseBlock(type);
        }
    }
}