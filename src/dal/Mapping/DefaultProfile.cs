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

            CreateMap<dal.Model.Transaction, dto.Model.TransactionWithBalance>()
                .ForMember(dest => dest.Balance, opt => opt.MapFrom(s => s.BalanceData.Balance))
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