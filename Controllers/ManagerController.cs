using AutoMapper;
using Common.Dto;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Service.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ManagersController : ControllerBase
{
    private readonly IManager _service;
    private readonly IMapper _mapper;

    public ManagersController(IManager service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ManagerDto>>> GetAll()
    {
        var dtos = await _service.GetAll();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManagerDto>> GetById(int id)
    {
        var dto = await _service.GetById(id);
        dto.ThrowIfNotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ManagerDto>> Create([FromBody] CreateManagerDto dto)
    {
        var managerDto = await _service.AddItem(dto);
        return CreatedAtAction(nameof(GetById), new { id = managerDto.Id }, managerDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ManagerDto>> Update(int id, [FromBody] UpdateManagerDto dto)
    {
        var updated = await _service.UpdateItem(id, dto);
      return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _service.DeleteItem(id);
        return NoContent();
    }
}
