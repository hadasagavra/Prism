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
public class WordsController : ControllerBase
{
    private readonly IService<WordDto> _service;
    private readonly IMapper _mapper;

    public WordsController(IService<WordDto> service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<Word>>> GetAll()
    {
        var dtos = await _service.GetAll();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Word>> GetById(int id)
    {
        var dto = await _service.GetById(id);
        dto.ThrowIfNotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<Word>> Create([FromBody] WordDto dto)
    {
        var created = await _service.AddItem(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.WordId }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Word>> Update(int id, [FromBody] WordDto dto)
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
