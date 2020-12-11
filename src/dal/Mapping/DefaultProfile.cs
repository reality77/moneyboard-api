using System.Linq;
using AutoMapper;

namespace dal.Mapping
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<dal.Model.TransactionRecognitionRule, dto.Model.TransactionRecognitionRuleEdit>()
                .ReverseMap();

            CreateMap<dal.Model.TransactionRecognitionRule, dto.Model.TransactionRecognitionRuleBase>();
            
            CreateMap<dal.Model.TransactionRecognitionRule, dto.Model.TransactionRecognitionRule>();
            
            CreateMap<dal.Model.TransactionRecognitionRuleAction, dto.Model.TransactionRecognitionRuleAction>()
                .ReverseMap();
            
            CreateMap<dal.Model.TransactionRecognitionRuleCondition, dto.Model.TransactionRecognitionRuleCondition>()
                .ReverseMap();

            CreateMap<dal.Model.ImportedFile, dto.Model.ImportedFile>();

            CreateMap<dal.Model.Account, dto.Model.AccountBase>();
            
            CreateMap<dal.Model.Account, dto.Model.AccountDetails>();

            CreateMap<dal.Model.Tag, dto.Model.Tag>()
                .ReverseMap();
            
            CreateMap<dal.Model.Tag, dto.Model.TagDetails>()
                .ForMember(dest => dest.ParentKey, opt => opt.MapFrom(s => s.ParentTag.Key))
                .ForMember(dest => dest.SubKeys, opt => opt.MapFrom(s => s.SubTags.Select(st => st.Key)))
                .ReverseMap();

            CreateMap<dal.Model.TagType, dto.Model.TagType>()
                .ReverseMap();               

            CreateMap<dto.Model.TagTypeEdit, dal.Model.TagType>();

            CreateMap<dal.Model.TransactionTag, dto.Model.TransactionTag>()
                .ReverseMap();

            CreateMap<dal.Model.ImportedTransaction, dto.Model.ImportedTransaction>()
                .ReverseMap();

            CreateMap<dal.Model.Transaction, dto.Model.Transaction>();

            CreateMap<dal.Model.ImportedTransaction, dto.Model.TransactionWithBalance>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(s => s.BalanceData.Balance));

            CreateMap<dal.Model.Transaction, dto.Model.TransactionWithBalance>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(s => s.BalanceData.Balance));
        }
    }
}