using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        
        public CharacterService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            Character character = _mapper.Map<Character>(newCharacter);
            
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            serviceResponse.Data = await _context.Characters.Select(x=> _mapper.Map<GetCharacterDto>(x)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try {
                Character character = _context.Characters.First(x=>x.Id == id);
                _context.Characters.Remove(character);
                await _context.SaveChangesAsync();

                serviceResponse.Data = _context.Characters.Select(x=> _mapper.Map<GetCharacterDto>(x)).ToList();
            } 
            catch(Exception ex) {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var response = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters.ToListAsync();
            response.Data = dbCharacters.Select(x => _mapper.Map<GetCharacterDto>(x)).ToList();

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _context.Characters.FirstOrDefaultAsync(x=> x.Id == id);
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacterDto)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();

            try {
                Character? character = await _context.Characters.FirstOrDefaultAsync(x=>x.Id == updateCharacterDto.Id);

                // _mapper.Map(updateCharacterDto, character);
                character.Name = updateCharacterDto.Name;
                character.HitPoints = updateCharacterDto.HitPoints;
                character.Defense = updateCharacterDto.Defense;
                character.Strength = updateCharacterDto.Strength;
                character.Intelligence = updateCharacterDto.Intelligence;
                character.Class = updateCharacterDto.Class;

                await _context.SaveChangesAsync();
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