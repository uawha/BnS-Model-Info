using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//version 2016.05.22
namespace Elan.Generic
{
    namespace EqualityComparer
    {
        public class StructuralEqualityComparer<T> : IEqualityComparer<T>
            where T : IStructuralEquatable
        {
            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                return ((IStructuralEquatable)x).Equals(y, StructuralComparisons.StructuralEqualityComparer);
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
            }
        }

        public class SetEqualityComparer<TElement, TSet> : IEqualityComparer<ISet<TElement>>
            where TSet : ISet<TElement>
        {
            bool IEqualityComparer<ISet<TElement>>.Equals(ISet<TElement> x, ISet<TElement> y)
            { return x.SetEquals(y); }

            int IEqualityComparer<ISet<TElement>>.GetHashCode(ISet<TElement> obj)
            {
                if (obj?.Count > 0)
                {
                    int init = obj.ElementAt(0).GetHashCode();
                    if (obj.Count > 1)
                    {
                        int i = 0;
                        foreach (var s in obj)
                        {
                            if (i > 0) init = init ^ s.GetHashCode();
                            i++;
                        }
                        return init;
                    }
                    else return init;
                }
                else return 0;
            }
        }

        public class HashSetEqualityComparer<TElement> : IEqualityComparer<HashSet<TElement>>
        {
            bool IEqualityComparer<HashSet<TElement>>.Equals(HashSet<TElement> x, HashSet<TElement> y)
            { return x.SetEquals(y); }

            int IEqualityComparer<HashSet<TElement>>.GetHashCode(HashSet<TElement> obj)
            {
                if (obj?.Count > 0)
                {
                    int init = obj.ElementAt(0).GetHashCode();
                    if (obj.Count > 1)
                    {
                        int i = 0;
                        foreach (var s in obj)
                        {
                            if (i > 0) init = init ^ s.GetHashCode();
                            i++;
                        }
                        return init;
                    }
                    else return init;
                }
                else return 0;
            }
        }
    }

    public interface ITreeNode<out T>
    {
        bool Is_RootNode { get; }
        bool Is_LeafNode { get; }
        T NodeContent { get; }
        IEnumerable<ITreeNode<T>> SubNodes { get; }
    }

    public interface IValuedTreeNode<out TNode, out TValue> : ITreeNode<TNode>
    {
        TValue Get_Value();
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

    public static class MISC
    {
        public static string GetPathable_YMD(DateTime date_time)
        {
            return $"{date_time.Year}{date_time.Month.ToString().PadLeft(2, '0')}{date_time.Day.ToString().PadLeft(2, '0')}";
        }
        public static string GetPathable_HM(DateTime date_time)
        {
            return $"{date_time.Hour.ToString().PadLeft(2, '0')}{ date_time.Minute.ToString().PadLeft(2, '0')}";
        }
        public static string GetPathable_TMDHM(DateTime date_time)
        {
            return $"{GetPathable_YMD(date_time)}{GetPathable_HM(date_time)}";
        }
        public static string GetSpaceString_ofLength(int num)
        {
            if (num < 1)
            {
                return String.Empty;
            }
            else
            {
                var sb = new StringBuilder();
                for (int j = 0; j < num; j++)
                {
                    sb.Append(" ");
                }
                return sb.ToString();
            }
        }

        public static long TryGet_FileLength(string file_path)
        {
            long _R;
            try
            {
                _R = (new FileInfo(file_path)).Length;
            }
            catch
            {
                _R = -1;
            }
            return _R;
        }

        public static string GetString_ofFileLength(string file_path)
        {
            long size = TryGet_FileLength(file_path);
            if (size > 0)
            {
                return $"{String.Format("{0:N0}", size)} bytes";
            }
            else
            {
                return "fail to get file size";
            }
        }
    }
}
