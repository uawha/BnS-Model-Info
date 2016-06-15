using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//version 2016.06.11
namespace Elan.Generic
{
    public class EqualityComparer
    {
        public class ConstructEqualityComparer<T> : IEqualityComparer<T>
        {
            public bool Equals(T x, T y)
            {
                return Equals_Core(x, y);
            }
            public int GetHashCode(T obj)
            {
                return Hash_Core(obj);
            }
            public Func<T, T, bool> Equals_Core;
            public Func<T, int> Hash_Core;
            public static ConstructEqualityComparer<T> By(
                Func<T, T, bool> equal_condition,
                Func<T, int> hash_func)
            {
                return new ConstructEqualityComparer<T>() {
                    Equals_Core = equal_condition,
                    Hash_Core = hash_func
                };
            }
        }

        /// <summary>
        /// A simple wrapper around IStructuralEquatable for generic `Key`.
        /// </summary>
        /// <typeparam name="T">type of element to compare</typeparam>
        public class StructuralEqualityComparer<T> : IEqualityComparer<T>
            where T : IStructuralEquatable
        {
            public bool Equals(T x, T y)
            {
                return x.Equals(y, StructuralComparisons.StructuralEqualityComparer);
            }
            public int GetHashCode(T obj)
            {
                return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
            }

            public static StructuralEqualityComparer<T> Get()
            {
                return new StructuralEqualityComparer<T>();
            }
        }

        /// <summary>
        /// As the System.Array. For not using ToArray() and
        /// fetch a StructuralEqualityComparer for that.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TList"></typeparam>
        public class iListEqualityComparer<TElement, TList> : IEqualityComparer<TList>
            where TList : IList<TElement>
            where TElement : IEquatable<TElement>
        {
            // From System.Web.Util.HashCodeCombiner
            // From System.Array
            static int CombineHashCodes(int h1, int h2)
            {
                return (((h1 << 5) + h1) ^ h2);
            }

            public bool Equals(TList x, TList y)
            {
                if (x?.Count > 0 && y?.Count > 0 && x.Count == y.Count)
                {
                    if (Object.ReferenceEquals(x, y)) return true;
                    for (int i = 0; i < x.Count; i += 1)
                    {
                        if (!x[i].Equals(y[i])) return false;
                    }
                    return true;
                }
                else return false;
            }

            public int GetHashCode(TList obj)
            {
                int init = 0;
                for (int i = (obj.Count >= 7 ? obj.Count - 7 : 0); i < obj.Count; i += 1)
                {
                    init = CombineHashCodes(init, obj[i].GetHashCode());
                    // last section of 7 elements
                }
                return init;
            }

            public static iListEqualityComparer<TElement, TList> Get()
            {
                return new iListEqualityComparer<TElement, TList>();
            }
        }

        public class ListEqualityComparer<TElement>
            where TElement : IEquatable<TElement>
        {
            public static iListEqualityComparer<TElement, List<TElement>> Get()
            {
                return new iListEqualityComparer<TElement, List<TElement>>();
            }
        }

        /// <summary>
        /// GetHashCode() use XOR;
        /// Equals() use SetEquals().
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TSet"></typeparam>
        public class iSetEqualityComparer<TElement, TSet> : IEqualityComparer<TSet>
            where TSet : ISet<TElement>
        {
            public bool Equals(TSet x, TSet y)
            {
                return x.SetEquals(y);
            }

            public int GetHashCode(TSet obj)
            {
                if (obj?.Count > 0)
                {
                    int init = 0;
                    foreach (var s in obj)
                    {
                        init = init ^ s.GetHashCode();
                        // we must take all into account...coz it's SET.
                        // while we can take a first or last section of
                        // a list to generate its hash, it not possible
                        // to do the same for set, coz it will break the
                        // definition of hash function. e.g.:
                        // Set A: 1,2,3,4,5
                        // Set B: 2,5,4,3,1
                    }
                    return init;
                }
                else return 0;
            }

            // sometimes I prefer not to use `new'.
            public static iSetEqualityComparer<TElement, TSet> Get()
            {
                return new iSetEqualityComparer<TElement, TSet>();
            }
        }

        public class HashSetEqualityComparer<TElement>
        {
            public static iSetEqualityComparer<TElement, HashSet<TElement>> Get()
            {
                return new iSetEqualityComparer<TElement, HashSet<TElement>>();
            }
        }
    }

    namespace Comparer
    {
        public class ConstructComparer<T> : IComparer<T>
        {
            public int Compare(T x, T y)
            {
                return Core(x, y);
            }
            public Func<T, T, int> Core;
            public static ConstructComparer<T> By(Func<T, T, int> compare_func)
            {
                return new ConstructComparer<T>() { Core = compare_func };
            }
        }
    }

    namespace Tree
    {
        public interface ITree<out TNode>
        {
            bool Is_RootNode { get; }
            bool Is_LeafNode { get; }
            TNode NodeContent { get; }
            IEnumerable<ITree<TNode>> SubTrees { get; }
        }

        public interface IValuedTree<out TNode, out TValue> : ITree<TNode>
        {
            TValue Get_Value();
        }

        #region 1+2*3
        /// <summary>
        /// if Is_Value, implement Value; else, Operation
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        public interface IOperationTree_Node<TValue>
        {
            bool Is_Value { get; }
            TValue Value { get; }
            Func<IEnumerable<ITree<IOperationTree_Node<TValue>>>, TValue> Operation { get; }
        }
        /// <summary>
        /// since c sharp abstract class and interface cannot supply implementation...
        /// </summary>
        public static class OperationTree_EX
        {
            public static TValue ComputeValue<TValue>(this ITree<IOperationTree_Node<TValue>> input)
            {
                if (input.NodeContent.Is_Value)
                {
                    return input.NodeContent.Value;
                }
                else
                {
                    return input.NodeContent.Operation(input.SubTrees);
                }
            }
        }

        #endregion
    }

    public static class Index_Method_EX
    {
        /* use FindIndex().
        /// <summary>
        /// returns an index >= start_index. -1 means none next.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="start_index"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static int Get_NextIndex_Where<T>(this List<T> input, int start_index, Func<T, bool> condition)
        {
            for (int cursor = start_index; cursor < input.Count; cursor++)
                if (condition(input[cursor])) return cursor;
            return -1;
        }

        public static int Get_NextIndex_Where<T>(this T[] input, int start_index, Func<T, bool> condition)
        {
            for (int cursor = start_index; cursor < input.Length; cursor++)
                if (condition(input[cursor])) return cursor;
            return -1;
        }

        public static int Get_Index_Where<T>(this List<T> input, Func<T, bool> condition)
        {
            return Get_NextIndex_Where(input, 0, condition);
        }

        public static int Get_Index_Where<T>(this T[] input, Func<T, bool> condition)
        {
            return Get_NextIndex_Where(input, 0, condition);
        }
        */
    }

    public static class Enumerable_EX
    {
        #region Reverse Map
        /// <summary>
        /// all elements in the domain are in the rev map's range, even if the domain you pass in is not a set
        /// regards to some equality comparer.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <typeparam name="TRange"></typeparam>
        /// <param name="domain"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static Dictionary<TRange, List<TDomain>> Get_ReverseMap_of<TDomain, TRange>(
            this IEnumerable<TDomain> domain, Func<TDomain, TRange> map)
        {
            var _R = new Dictionary<TRange, List<TDomain>>();
            foreach (TDomain x in domain)
            {
                TRange y = map(x);
                if (_R.ContainsKey(y))
                {
                    _R[y].Add(x);
                }
                else
                {
                    var v = new List<TDomain>();
                    v.Add(x);
                    _R.Add(y, v);
                }
            }
            return _R;
        }

        /// <summary>
        /// this is a take-one version of the previous one.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <typeparam name="TRange"></typeparam>
        /// <param name="domain"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static Dictionary<TRange, TDomain> Get_ReverseSampleMap_of<TDomain, TRange>(
            this IEnumerable<TDomain> domain, Func<TDomain, TRange> map)
        {
            var _R = new Dictionary<TRange, TDomain>();
            foreach (TDomain x in domain)
            {
                TRange y = map(x);
                if (!_R.ContainsKey(y))
                {
                    _R.Add(y, x);
                }
            }
            return _R;
        }
        #endregion

        #region Join Map
        public static Dictionary<TKey, HashSet<TValue>> JoinMap_RangeAsSet<TSource, TKey, TValue>(
            this IEnumerable<TSource> source_collection,
            Func<TSource, TKey> src_key_map, Func<TSource, TValue> src_value_map,
            IEqualityComparer<TKey> equality_comparer = null)
        {
            Dictionary<TKey, HashSet<TValue>> _R;
            if (equality_comparer == null) _R = new Dictionary<TKey, HashSet<TValue>>();
            else _R = new Dictionary<TKey, HashSet<TValue>>(equality_comparer);
            foreach (TSource s in source_collection)
            {
                TKey x = src_key_map(s);
                TValue y = src_value_map(s);
                if (_R.ContainsKey(x))
                {
                    _R[x].Add(y);
                }
                else
                {
                    var v = new HashSet<TValue>();
                    v.Add(y);
                    _R.Add(x, v);
                }
            }
            return _R;
        }


        public static Dictionary<TKey, List<TValue>> JoinMap_RangeAsList<TSource, TKey, TValue>(
            this IEnumerable<TSource> source_collection,
            Func<TSource, TKey> src_key_map, Func<TSource, TValue> src_value_map,
            IEqualityComparer<TKey> equality_comparer = null)
        {
            Dictionary<TKey, List<TValue>> _R;
            if (equality_comparer == null) _R = new Dictionary<TKey, List<TValue>>();
            else _R = new Dictionary<TKey, List<TValue>>(equality_comparer);
            foreach (TSource s in source_collection)
            {
                TKey x = src_key_map(s);
                TValue y = src_value_map(s);
                if (_R.ContainsKey(x))
                {
                    _R[x].Add(y);
                }
                else
                {
                    var v = new List<TValue>();
                    v.Add(y);
                    _R.Add(x, v);
                }
            }
            return _R;
        }
        #endregion
        /// <summary>
        /// works like String.Substring()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static List<T> Sublist<T>(this List<T> input, int startIndex, int length)
        {
            var _R = new List<T>();
            for (int i = startIndex; i < startIndex + length; i++) _R.Add(input[i]);
            return _R;
        }

        public static List<T> Sublist<T>(this List<T> input, int startIndex)
        {
            var _R = new List<T>();
            for (int i = startIndex; i < input.Count; i++) _R.Add(input[i]);
            return _R;
        }

        /*
        /// <summary>
        /// all elements in the domain are in the rev map's range, even if the domain you pass in is not a set
        /// regards to some equality comparer. If possible, use ToLookUp instead of this. The method exist
        /// because ToLookUp is not offline documented.
        /// </summary>
        /// <typeparam name="TDomain"></typeparam>
        /// <typeparam name="TRange"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="domain"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static Dictionary<TRange, List<TDomain>> Get_ReverseMap_of<TDomain, TRange, TSource>(
           this IEnumerable<TDomain> domain, Func<TDomain, TRange> map)
        {
            var _R = new Dictionary<TRange, List<TDomain>>();
            foreach (TDomain x in domain)
            {
                TRange y = map(x);
                if (_R.ContainsKey(y))
                {
                    _R[y].Add(x);
                }
                else
                {
                    var v = new List<TDomain>();
                    v.Add(x);
                    _R.Add(y, v);
                }
            }
            return _R;
        }
        */
        //fluent syntax, yeah.
        public static HashSet<T> ToSet<T>(this IEnumerable<T> input)
        {
            return new HashSet<T>(input);
        }

        public static HashSet<T> SubsetBy<T>(this IEnumerable<T> input, Func<T, bool> condition)
        {
            var _R = new HashSet<T>();
            foreach (T x in input)
                if (condition(x)) _R.Add(x);
            return _R;
        }

        #region LookUps
        // since ILookup is not offline doced, do not use.
        public static Dictionary<TKey, List<TElement>> ToMap_RangeAsList<TKey, TElement>(this ILookup<TKey, TElement> input)
        {
            IEnumerable<IGrouping<TKey, TElement>> u_ref = input;
            HashSet<TKey> key_set = u_ref.Select(x => x.Key).ToSet();
            var _R = new Dictionary<TKey, List<TElement>>();
            foreach (var v in key_set) _R.Add(v, input[v].ToList());
            return _R;
        }

        public static Dictionary<TKey, HashSet<TElement>> ToMap_RangeAsSet<TKey, TElement>(this ILookup<TKey, TElement> input)
        {
            IEnumerable<IGrouping<TKey, TElement>> u_ref = input;
            HashSet<TKey> key_set = u_ref.Select(x => x.Key).ToSet();
            var _R = new Dictionary<TKey, HashSet<TElement>>();
            foreach (var v in key_set) _R.Add(v, input[v].ToSet());
            return _R;
        }

        public static SortedDictionary<TKey, List<TElement>> ToSortedMap_RangeAsList<TKey, TElement>(this ILookup<TKey, TElement> input)
        {
            IEnumerable<IGrouping<TKey, TElement>> u_ref = input;
            HashSet<TKey> key_set = u_ref.Select(x => x.Key).ToSet();
            var _R = new SortedDictionary<TKey, List<TElement>>();
            foreach (var v in key_set) _R.Add(v, input[v].ToList());
            return _R;
        }

        public static SortedDictionary<TKey, HashSet<TElement>> ToSortedMap_RangeAsSet<TKey, TElement>(this ILookup<TKey, TElement> input)
        {
            IEnumerable<IGrouping<TKey, TElement>> u_ref = input;
            HashSet<TKey> key_set = u_ref.Select(x => x.Key).ToSet();
            var _R = new SortedDictionary<TKey, HashSet<TElement>>();
            foreach (var v in key_set) _R.Add(v, input[v].ToSet());
            return _R;
        }
        #endregion
        //the depth 1 collection might be anything
        public static List<T> MakeUnion<T>(this IEnumerable<IEnumerable<T>> input)
        {
            var _R = new List<T>();
            foreach (IEnumerable<T> c in input)
                foreach (T s in c)
                    if (!_R.Contains(s)) _R.Add(s);
            return _R;
        }

        public static List<TElement> Get_Min_by<TElement, TCompareKey>(this IEnumerable<TElement> input, Func<TElement, TCompareKey> compareKey_map)
            where TCompareKey : IComparable<TCompareKey>
        {
            var dict = input.Get_ReverseMap_of(compareKey_map);
            TCompareKey min_key = dict.Keys.Min();
            return dict[min_key];
        }

        public static List<TElement> Get_Max_by<TElement, TCompareKey>(this IEnumerable<TElement> input, Func<TElement, TCompareKey> compareKey_map)
            where TCompareKey : IComparable<TCompareKey>
        {
            var dict = input.Get_ReverseMap_of(compareKey_map);
            TCompareKey min_key = dict.Keys.Max();
            return dict[min_key];
        }

        /// <summary>
        /// e.g.
        /// input = {"a","b","c"};
        /// link_string = ", ";
        /// you get "a, b, c"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="link_string"></param>
        /// <returns></returns>
        public static string Concatenate_with<T>(this IEnumerable<T> input, string link_string)
        {
            var sb = new StringBuilder();
            foreach (var v in input)
            {
                sb.Append(v.ToString());
                sb.Append(link_string);
            }
            if (sb.Length != 0) sb.Remove(sb.Length - link_string.Length, link_string.Length);
            return sb.ToString();
        }

        public static bool FindFirst_byCondition<T>(this IEnumerable<T> input, Func<T, bool> condition, out T result)
        {
            foreach (var v in input)
            {
                if (condition(v))
                {
                    result = v;
                    return true;
                }
            }
            result = default(T);
            return false;
        }
    }
}
