using System;
using System.Collections.Generic;
using System.Linq;

namespace PmSoft
{
    /// <summary>
    /// 封装用于分页的实体Id 
    /// </summary>
    [Serializable]
    public class PagingEntityIdCollection
    {
        private List<object> entityIds;
        private bool isContainsMultiplePages;
        private int totalRecords;

        /// <summary>
        /// 构造函数 
        /// </summary>
        /// <param name="entityIds">实体ID集合</param>
        public PagingEntityIdCollection(IEnumerable<object> entityIds)
        {
            this.totalRecords = -1;
            this.entityIds = entityIds.ToList<object>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entityIds">实体ID集合</param>
        /// <param name="totalRecords">总记录数</param>
        public PagingEntityIdCollection(IEnumerable<object> entityIds, int totalRecords)
        {
            this.totalRecords = -1;
            this.entityIds = entityIds.ToList<object>();
            this.totalRecords = totalRecords;
        }

        /// <summary>
        /// 获取pageIndex所在页数的EntityId集合 
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IEnumerable<object> GetPagingEntityIds(int pageSize, int pageIndex)
        {
            if (this.entityIds == null)
            {
                return new List<object>();
            }
            if (!this.IsContainsMultiplePages)
            {
                return this.entityIds.GetRange(0, (this.Count > pageSize) ? pageSize : this.Count);
            }
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            int index = pageSize * (pageIndex - 1);
            int num2 = pageSize * pageIndex;
            int count = this.entityIds.Count;
            if (index >= count)
            {
                return new List<object>();
            }
            if (num2 < count)
            {
                return this.entityIds.GetRange(index, pageSize);
            }
            return this.entityIds.GetRange(index, count - index);
        }

        /// <summary>
        /// 获取前topNumber条EntityId集合 
        /// </summary>
        /// <param name="topNumber">前多少条数据</param>
        /// <returns></returns>
        public IEnumerable<object> GetTopEntityIds(int topNumber)
        {
            if (this.entityIds == null)
            {
                return new List<object>();
            }
            int count = this.entityIds.Count;
            return this.entityIds.GetRange(0, (this.Count > topNumber) ? topNumber : this.Count);
        }

        /// <summary>
        /// 获取的entityId数量 
        /// </summary>
        public int Count
        {
            get
            {
                if (this.entityIds == null)
                {
                    return 0;
                }
                return this.entityIds.Count;
            }
        }

        /// <summary>
        /// 是否包含前N页数据
        /// </summary>
        public bool IsContainsMultiplePages
        {
            get
            {
                return this.isContainsMultiplePages;
            }
            set
            {
                this.isContainsMultiplePages = value;
            }
        }

        /// <summary>
        /// 符合查询条件的总记录数 
        /// </summary>
        public int TotalRecords
        {
            get
            {
                if (this.totalRecords > 0L)
                {
                    return this.totalRecords;
                }
                return this.Count;
            }
        }
    }
}

