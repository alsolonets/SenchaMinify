using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SenchaMinify.Library
{
    public class SenchaOrderer
    {
        /// <summary>
        /// Dependency resolving using topological sort
        /// </summary>
        /// <param name="node">File to start from</param>
        /// <param name="resolved">Collection of resolved files</param>
        protected virtual void DependencyResolve<TNode>(TNode node, IList<TNode> resolved)
            where TNode: SenchaFileNode
        {
            node.Color = SenchaFileNode.SortColor.Gray;

            foreach (TNode dependency in node.Dependencies)
            {
                if (dependency.Color == SenchaFileNode.SortColor.White)
                {
                    DependencyResolve(dependency, resolved);
                }
                else if (dependency.Color == SenchaFileNode.SortColor.Gray)
                {
                    throw new InvalidOperationException(String.Format(
                        "Circular dependency detected: '{0}' -> '{1}'", 
                        node.ClassName ?? String.Empty, 
                        dependency.ClassName ?? String.Empty)
                    );
                }
            }

            node.Color = SenchaFileNode.SortColor.Black;
            resolved.Add(node);
        }

        /// <summary>
        /// Order Sencha file nodes using topological sort
        /// </summary>
        /// <param name="files">SenchaFileInfo wrappers</param>
        /// <returns>Ordered SenchaFileInfo wrappers</returns>
        public virtual IEnumerable<TNode> OrderSenchaFiles<TNode>(IEnumerable<TNode> files)
            where TNode: SenchaFileNode
        {
            // Fill dependencies for each wrapper
            files.ToList().ForEach(ef => ef.FillDependencies(files));
            
            // Resolving dependencies
            IList<TNode> unresolved = files.ToList();
            IList<TNode> resolved = new List<TNode>();

            TNode startNode = unresolved
                .Where(ef => ef.Color == SenchaFileNode.SortColor.White)
                .FirstOrDefault();

            while (startNode != null)
            {
                DependencyResolve(startNode, resolved);
                startNode = unresolved
                    .Where(ef => ef.Color == SenchaFileNode.SortColor.White)
                    .FirstOrDefault();
            }

            return resolved;
        }
    }
}