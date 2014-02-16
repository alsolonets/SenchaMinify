using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SenchaMinify
{
    public class SenchaOrderer
    {
        /// <summary>
        /// Dependency resolving using topological sort
        /// </summary>
        /// <param name="node">File to start from</param>
        /// <param name="resolved">Collection of resolved files</param>
        protected virtual void DependencyResolve<TNode>(TNode node, IList<TNode> resolved)
            where TNode: SenchaFile
        {
            node.Color = SenchaFile.SortColor.Gray;

            foreach (TNode dependency in node.Dependencies)
            {
                if (dependency.Color == SenchaFile.SortColor.White)
                {
                    DependencyResolve(dependency, resolved);
                }
                else if (dependency.Color == SenchaFile.SortColor.Gray)
                {
                    throw new InvalidOperationException(String.Format(
                        "Circular dependency detected: '{0}' -> '{1}'", 
                        node.FullName ?? String.Empty, 
                        dependency.FullName ?? String.Empty)
                    );
                }
            }

            node.Color = SenchaFile.SortColor.Black;
            resolved.Add(node);
        }

        /// <summary>
        /// Order Sencha file nodes using topological sort
        /// </summary>
        /// <param name="files">SenchaFileInfo wrappers</param>
        /// <returns>Ordered SenchaFileInfo wrappers</returns>
        public virtual IEnumerable<TNode> OrderFiles<TNode>(IEnumerable<TNode> files)
            where TNode: SenchaFile
        {
            var filelist = files.ToList();

            // Fill dependencies for each wrapper
            filelist.ForEach(ef => ef.FillDependencies(filelist));

            // Resolving dependencies
            IList<TNode> unresolved = filelist;
            IList<TNode> resolved = new List<TNode>();

            TNode startNode = unresolved
                .Where(ef => ef.Color == SenchaFile.SortColor.White)
                .FirstOrDefault();

            while (startNode != null)
            {
                DependencyResolve(startNode, resolved);
                startNode = unresolved
                    .Where(ef => ef.Color == SenchaFile.SortColor.White)
                    .FirstOrDefault();

            }

            return resolved;
        }
    }
}