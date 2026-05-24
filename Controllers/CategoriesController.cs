using AutoMapper;
using Common.Dto;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interfaces;
[ApiController]
[Route("api/[controller]")]

public class CategoriesController : ControllerBase
{
    private readonly IService<CategoryDto> _service;
    private readonly IMapper _mapper;

    public CategoriesController(IService<CategoryDto> service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var dtos = await _service.GetAll();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var dto = await _service.GetById(id);
        dto.ThrowIfNotFound();
        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto)
    {
        var created = await _service.AddItem(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryDto dto)
    {
        var updated = await _service.UpdateItem(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(int id)
    {
        await _service.DeleteItem(id);
        return NoContent();
    }
}
