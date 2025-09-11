drop database if exists bdBiblioteca;
create database bdBiblioteca;
use bdBiblioteca;

create table Usuarios(
	id int primary key auto_increment,
	nome varchar(100),
	email varchar(100),
	senha_Hash varchar(255),
	role enum ("Bibliotecario","Admin"),
	ativo tinyint(1) default 1,
	criado_em datetime default current_timestamp
);

delimiter $$
drop procedure if exists sp_usuario_criar $$
create procedure sp_usuario_criar (
    in p_nome varchar(100),
    in p_email varchar(100),
    in p_senha_hash varchar(255),
    in p_role varchar(20) 
)
begin
    insert into Usuarios (nome, email, senha_Hash, role, ativo, criado_em)
    values (p_nome, p_email, p_senha_hash, p_role, 1, NOW());
end $$

call sp_usuario_criar(
    'João Admin',
    'joao@biblioteca.com',
    '$2a$11$HASHADMINEXEMPLO9876543210',
    'Admin'
);

select * from usuarios;

create table Editoras(
	id int primary key auto_increment,
    nome varchar(150) not null,
    criado_em datetime not null default current_timestamp
);

create table Generos(
	id int primary key auto_increment,
    nome varchar(100) not null,
    criado_em datetime not null default current_timestamp
);

create table Autores(
	id int primary key auto_increment,
    nome varchar(150) not null,
    criado_em datetime not null default current_timestamp
);

create table Livros(
	id int primary key auto_increment,
    titulo varchar(200) not null,
    autorId int,
    editoraId int,
    generoId int,
    ano smallint,
    isbn varchar(32),
    quantidade_total int,
    quantidade_disponivel int,
    criado_em datetime not null default current_timestamp
);

-- Criando as fks
Alter Table Livros
	add constraint fk_livros_autor
		foreign key(autorId) references Autores(id),
	add constraint fk_livros_editora
		foreign key(editoraId) references Editoras(id),
	add constraint fk_livros_genero
		foreign key(GeneroId) references Generos(id);


delimiter $$
drop procedure if exists sp_editora_criar $$
create procedure sp_editora_criar(in p_nome varchar(150))
begin
	insert into Editoras (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_genero_criar $$
create procedure sp_genero_criar(in p_nome varchar(100))
begin
	insert into Generos (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_autor_criar $$
create procedure sp_autor_criar(in p_nome varchar(150))
begin
	insert into Autores (nome, criado_em) values (p_nome, NOW());
end;
$$

delimiter $$
drop procedure if exists sp_autor_listar $$
create procedure sp_autor_listar()
begin
	select id, nome from Autores order by nome;
end; $$

delimiter $$
drop procedure if exists sp_editora_listar $$
create procedure sp_editora_listar()
begin
	select id, nome from Editoras order by nome;
end; $$

delimiter $$
drop procedure if exists sp_genero_listar $$
create procedure sp_genero_listar()
begin
	select id, nome from Generos order by nome;
end; $$

delimiter $$
drop procedure if exists sp_livro_criar $$
create procedure sp_livro_criar (
	in p_titulo varchar(200),
    in p_autor int,
    in p_editora int,
    in p_genero int,
    in p_ano smallint,
    in p_isbn varchar(32),
    in p_quantidade int)
begin
	insert into Livros(titulo, autorId, editoraId, generoId, ano, isbn, quantidade_total, quantidade_disponivel)
				values(p_titulo, p_autor, p_editora, p_genero, p_ano, p_isbn,p_quantidade, p_quantidade);
end; $$

delimiter $$
drop procedure if exists sp_livro_listar $$
create procedure sp_livro_listar ()
begin
	select
		l.id,
        l.titulo,
        l.autorId,
        a.nome as autor_nome,
        l.editoraId,
        e.nome as editora_nome,
        l.generoId,
        g.nome as genero_nome,
        l.ano,
        l.isbn,
        l.quantidade_total,
        l.quantidade_disponivel,
        l.criado_em
	from livros l
    left join autores   a on a.id = l.autorId
    left join editoras e on e.id = l.editoraId
    left join generos  g on g.id = l.generoId
    order by l.titulo;
end; $$


Delimiter $$
drop procedure if exists sp_usuario_obter_por_email $$
Create Procedure sp_usuario_obter_por_email(In p_email varchar(100))
begin
	select id, nome, email, senha_hash, role, ativo
    from usuarios
    where email = p_email
    limit 1;
end$$

Delimiter ;




Delimiter $$
Drop Procedure if exists sp_livro_obter $$
Create procedure sp_livro_obter (In p_id int)
begin
	select id, titulo, autorId, editoraId, generoId, ano, isbn
			quantidade_total, quantidade_disponivel, criado_em
    From Livros where id = p_id;
End;

drop procedure if exists sp_livro_atualizar $$
create procedure sp_livro_atualizar(
in p_id int, in p_titulo varchar(200), in p_autor int, in p_editora int,
in p_genero int, in p_ano smallint, in p_isbn varchar(32), in p_novo_total int)
begin
	declare v_disp int; declare v_total int;
    select quantidade_disponivel, quantidade total into v_disp, v_total
    from Livros where id = p_id for Update;

	Update Livros
    set titulo = p_titulo, autorId = p_autor, editoraId = p_editora, genero = p_genero,
		ano = p_ano, isbn = p_isbn,
        quantidade_total = p_novo_total,
        quantidade_disponivel = GREATEST(0, LEAST(p_novo_total, v_disp + (p_novo_total - v_total)))
        where id = p_id;
end;

drop procedure if exists sp_livro_excluir $$
create procedure sp_livro_excluir (in p_id int)
begin
	delete from Livros where id = p_id;
end $$

delimiter ;




select * from Livros;
select * from Usuarios;
select * from Generos;
Select * from Editoras;
select * from Autores;