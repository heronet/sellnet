using System.Collections.Generic;

namespace sellnet.DTO
{
    public class GetResponseWithPageDTO<T>
    {
        public GetResponseWithPageDTO(List<T> data, long size)
        {
            Data = data;
            Size = size;
        }

        public List<T> Data { get; set; }
        public long Size { get; set; }
    }
}