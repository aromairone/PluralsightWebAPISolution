using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MyCodeCamp.Data.Entities;

namespace PluralsightWebAPI.Models
{
    public class CampMappingProfile : Profile
    {
        //Start Date and EndDate are custom properties of the model
        //With Auto mapper we can tell it to map EventDate to Start Date
        //EndDate is calculated taking EventDate value and adding the Lenght of the Camp as No. of days
        public CampMappingProfile()
        {
            CreateMap<Camp, CampModel>()
                .ForMember(c => c.StartDate,
                opt => opt.MapFrom(camp => camp.EventDate))
                .ForMember(c => c.EndDate,
                opt => opt.ResolveUsing(camp => camp.EventDate.AddDays(camp.Length - 1)));                
        }
    }
}
