using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private int getUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            Character character = _mapper.Map<Character>(newCharacter);
            character.User = await _context.Users.FirstOrDefaultAsync(x=>x.Id == getUserId());
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

            serviceResponse.Data = await _context.Characters
                                            .Where(x=>x.User.Id == getUserId())
                                            .Select(x=> _mapper.Map<GetCharacterDto>(x)).ToListAsync();

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try {
                Character character = await _context.Characters.FirstOrDefaultAsync(x=>x.Id == id && x.User.Id == getUserId());

                if(character != null) {
                    _context.Characters.Remove(character);
                    await _context.SaveChangesAsync();

                    serviceResponse.Data = _context.Characters
                    .Where(x=> x.User.Id == getUserId())
                    .Select(x=> _mapper.Map<GetCharacterDto>(x)).ToList();
                }
                else {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Character Not Found!";
                }
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
            var dbCharacters = await _context.Characters
                    .Include(x => x.Weapon)
                    .Include(x => x.Skills)
                .Where(x => x.User.Id == getUserId())
                .ToListAsync();
            response.Data = dbCharacters.Select(x => _mapper.Map<GetCharacterDto>(x)).ToList();

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();
            var dbCharacter = await _context.Characters
                    .Include(x => x.Weapon)
                    .Include(x => x.Skills).FirstOrDefaultAsync(x=> x.Id == id && x.User.Id == getUserId());
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacterDto)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();

            try {
                Character? character = await _context.Characters
                        .Include(x=>x.User)
                        .FirstOrDefaultAsync(x=>x.Id == updateCharacterDto.Id);

                if(character.User.Id == getUserId()) {
                    character.Name = updateCharacterDto.Name;
                    character.HitPoints = updateCharacterDto.HitPoints;
                    character.Defense = updateCharacterDto.Defense;
                    character.Strength = updateCharacterDto.Strength;
                    character.Intelligence = updateCharacterDto.Intelligence;
                    character.Class = updateCharacterDto.Class;

                    await _context.SaveChangesAsync();
                    response.Data = _mapper.Map<GetCharacterDto>(character);
                } else {
                    response.Success = false;
                    response.Message = "Character Not Found!";
                }
            } 
            catch(Exception ex) {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();

            try
            {
                var character = await _context.Characters
                    .Include(x => x.Weapon)
                    .Include(x => x.Skills)
                    .FirstOrDefaultAsync(x=> x.Id == newCharacterSkill.CharacterId && x.User.Id == getUserId());

                if(character == null)
                {
                    response.Success = false;
                    response.Message = "Character Not Found!";

                    return response;
                }

                var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);

                if(skill == null)
                {
                    response.Success = false;
                    response.Message = "Skill Not Found!";

                    return response;
                }

                character.Skills.Add(skill);
                await _context.SaveChangesAsync();

                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
  }
}