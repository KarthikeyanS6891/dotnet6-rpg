using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        public FightService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            var response = new ServiceResponse<FightResultDto>() {
                Data = new FightResultDto()
            };

            try
            {
                var characters = await _dataContext.Characters.Include(w => w.Weapon).Include(w => w.Skills)
                                        .Where(w => request.CharacterIds.Contains(w.Id)).ToListAsync();
                
                bool defeated = false;

                while(!defeated)
                {
                    foreach(Character attacker in characters)
                    {
                        var opponents = characters.Where(c=> c.Id != attacker.Id).ToList();
                        var opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;

                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;
                        if(useWeapon)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);
                        }
                        else
                        {
                            var skill = attacker.Skills[new Random().Next(attacker.Skills.Count)];
                            attackUsed = skill.Name;
                            damage = DoSkillAttack(attacker, opponent, skill);
                        }
                        response.Data.Log
                                .Add($"{attacker.Name} attacks {opponent.Name} using {attackUsed} with {(damage >= 0 ? damage : 0)} damage");

                        if(opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;

                            response.Data.Log.Add($"{opponent.Name} has been defeated!");
                            response.Data.Log.Add($"{attacker.Name} wins with {attacker.HitPoints} HP Left!");
                            break;
                        }
                    }
                }

                characters.ForEach(c => {
                    c.Fights++;
                    c.HitPoints = 100;
                });

                await _dataContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();

            try
            {
                var attacker = await _dataContext.Characters.Include(x => x.Skills)
                                    .FirstOrDefaultAsync(x => x.Id == request.AttackerId);

                var opponent = await _dataContext.Characters
                                    .FirstOrDefaultAsync(x => x.Id == request.OpponentId);

                var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);

                if (skill == null)
                {
                response.Success = false;
                response.Message = $"{attacker.Name} doesn't know that skill";

                return response;
                }
                int damage = DoSkillAttack(attacker, opponent, skill);

                if (opponent.HitPoints <= 0)
                response.Message = $"{opponent.Name} has been defeated";

                await _dataContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                Attacker = attacker.Name,
                Opponent = opponent.Name,
                AttackerHP = attacker.HitPoints,
                OpponentHP = opponent.HitPoints,
                Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private static int DoSkillAttack(Character? attacker, Character? opponent, Skill? skill)
        {
            int damage = skill.DamageValue + (new Random().Next(attacker.Intelligence));
            damage -= new Random().Next(opponent.Defense);

            if (damage > 0)
                opponent.HitPoints -= damage;

            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            var response = new ServiceResponse<AttackResultDto>();

            try
            {
                var attacker = await _dataContext.Characters.Include(x => x.Weapon)
                                    .FirstOrDefaultAsync(x => x.Id == request.AttackerId);

                var opponent = await _dataContext.Characters.Include(x => x.Weapon)
                                    .FirstOrDefaultAsync(x => x.Id == request.OpponentId);

                int damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HitPoints <= 0)
                response.Message = $"{opponent.Name} has been defeated";

                await _dataContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                Attacker = attacker.Name,
                Opponent = opponent.Name,
                AttackerHP = attacker.HitPoints,
                OpponentHP = opponent.HitPoints,
                Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        private static int DoWeaponAttack(Character? attacker, Character? opponent)
        {
            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defense);

            if (damage > 0)
                opponent.HitPoints -= damage;
            return damage;
        }

        public async Task<ServiceResponse<List<HighScoreDto>>> GetHighScore()
        {
            var response = new ServiceResponse<List<HighScoreDto>>();

            var characters = await _dataContext.Characters
                    .Where(x => x.Fights > 0)
                    .OrderByDescending(x => x.Victories)
                    .ThenBy(x => x.Defeats)
                    .ToListAsync();

            response.Data = characters.Select(x => _mapper.Map<HighScoreDto>(x)).ToList();

            return response;
        }
  }
}