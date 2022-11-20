using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private static List<Character> characters = new List<Character>() {
            new Character(),
            new Character { Id = 1, Name = "Sam" }
        };
        private readonly IMapper _mapper;
        
        public CharacterService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            Character character = _mapper.Map<Character>(newCharacter);
            character.Id = characters.Max(x=>x.Id) +1;
            characters.Add(character);

            serviceResponse.Data = characters.Select(x=> _mapper.Map<GetCharacterDto>(x)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try {
                Character character = characters.First(x=>x.Id == id);
                characters.Remove(character);
                serviceResponse.Data = characters.Select(x=> _mapper.Map<GetCharacterDto>(x)).ToList();
            } 
            catch(Exception ex) {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            return new ServiceResponse<List<GetCharacterDto>> {Data = characters.Select(x=> _mapper.Map<GetCharacterDto>(x)).ToList()};
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var character = characters.FirstOrDefault(x=> x.Id == id);
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacterDto)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();

            try {
                Character character = characters.FirstOrDefault(x=>x.Id == updateCharacterDto.Id);

                // _mapper.Map(updateCharacterDto, character);
                character.Name = updateCharacterDto.Name;
                character.HitPoints = updateCharacterDto.HitPoints;
                character.Defense = updateCharacterDto.Defense;
                character.Strength = updateCharacterDto.Strength;
                character.Intelligence = updateCharacterDto.Intelligence;
                character.Class = updateCharacterDto.Class;

                response.Data = _mapper.Map<GetCharacterDto>(character);
            } 
            catch(Exception ex) {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
        
  }
}