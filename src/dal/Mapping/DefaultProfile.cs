using System.Linq;
using AutoMapper;

namespace dal.Mapping
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<dal.Model.Tag, dto.Model.Tag>();

            CreateMap<dal.Model.Transaction, dto.Model.Transaction>()
                .ForMember(dest => dest.Tags, 
                    opt => opt.MapFrom(s => s.TransactionTags.AsQueryable().Select(tag => new dto.Model.Tag
                    {
                        TypeKey = tag.Tag.TypeKey,
                        Key = tag.Tag.Key,
                        Caption = tag.Tag.Caption,
                    }
                )));
        }        
    }
}