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

select * from Livros;

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

-- Criando a coluna de capa imagens
alter table Livros
		Add column capa_arquivo varchar(255) null after isbn;


create table leitor(
id_leitor int primary key auto_increment,
nomeLeitor varchar(30),
foto_leitor varchar(255),
criado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);



Create table emprestimos(
id int primary key auto_increment,
id_leitor int not null,
id_bibliotecario int not null,
data_emprestimo datetime not null Default CURRENT_TIMESTAMP,
data_prevista_devolucao date not null,
data_devolucao_geral DATETIME null,
status enum ('Ativo','Finalizado','Parcial') not null default  'Ativo'
);

create table emprestimo_itens(
id int primary key auto_increment,
id_emprestimo int not null,
id_livro int not null,
quantidade int not null default 1,
data_devolucao_item datetime null
);


alter table emprestimo_itens
	add constraint fk_itens_emp foreign key(id_emprestimo) references emprestimos(id),
    add constraint fk_itens_livro foreign key (id_livro) references Livros(id);

alter table emprestimos
    add constraint fk_emprestimos_usuario foreign key (id_bibliotecario) references Usuarios(id),
	add constraint fk_leitor_emp foreign key(id_leitor) references leitor(id_leitor);






delimiter $$
drop procedure if exists sp_leitor_listar $$
create procedure sp_leitor_listar()
begin
	select id_leitor, nomeLeitor, foto_leitor
    from Leitor;
    
end $$

delimiter $$
drop procedure if exists sp_leitor_criar $$
create procedure sp_leitor_criar(p_nome varchar(30), p_foto varchar(255))
begin
			insert into leitor(nomeLeitor,foto_leitor, criado_em)
						values(p_nome,p_foto, now());
end $$


delimiter $$
drop procedure if exists sp_leitor_obter $$
create procedure sp_leitor_obter(l_id int)
begin
	select id_leitor, nomeLeitor, foto_leitor
    from Leitor
    where id_leitor = l_id;
end $$

Delimiter $$
drop procedure if exists sp_leitor_editar$$
create procedure sp_leitor_editar(l_id int, l_nome varchar(30), l_foto varchar(255))
begin

	Update Leitor
    set nomeLeitor = l_nome, foto_leitor = l_foto
    where id_leitor = l_id;



end $$

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
    in p_quantidade int,
    in p_capa_arquivo varchar(255))
begin
	insert into Livros(titulo, autorId, editoraId, generoId, ano, isbn, quantidade_total, quantidade_disponivel,capa_arquivo)
				values(p_titulo, p_autor, p_editora, p_genero, p_ano, p_isbn,p_quantidade, p_quantidade, p_capa_arquivo);
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
        l.capa_arquivo,
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
	select id, titulo, autorId, editoraId, generoId, ano, isbn,
			quantidade_total, quantidade_disponivel, criado_em, capa_arquivo
    From Livros where id = p_id;
End;

Delimiter $$
drop procedure if exists sp_livro_atualizar $$
create procedure sp_livro_atualizar(
in p_id int, in p_titulo varchar(200), in p_autor int, in p_editora int,
in p_genero int, in p_ano smallint, in p_isbn varchar(32), in p_novo_total int)
begin
	declare v_disp int; declare v_total int;
    select quantidade_disponivel, quantidade_total into v_disp, v_total
    from Livros where id = p_id for Update;

	Update Livros
    set titulo = p_titulo, autorId = p_autor, editoraId = p_editora, generoId = p_genero,
		ano = p_ano, isbn = p_isbn,
        quantidade_total = p_novo_total,
        quantidade_disponivel = GREATEST(0, LEAST(p_novo_total, v_disp + (p_novo_total - v_total)))
        where id = p_id;
end;

drop procedure if exists sp_livro_excluir;
Delimiter $$
create procedure sp_livro_excluir (in p_id int)
begin
	delete from Livros where id = p_id;
end $$

delimiter ;

-- Editora


Delimiter $$
Drop procedure if exists sp_select_editora $$
create procedure sp_select_editora(id_edi int)
begin
	select id, nome from Editoras where id = id_edi;
end $$

Drop procedure if exists sp_editar_editora;
Delimiter $$
create procedure sp_editar_editora(id_edi int, nome_edi varchar(150))
Begin

	Update Editoras
    set nome = nome_edi
	where id = id_edi;
    
end $$

Delimiter $$
Drop procedure if exists sp_deletar_editora $$
create procedure sp_deletar_editora(id_edi int)
begin

	delete from Editoras where id = id_edi;

end $$


-- AUTOR -------------------------------


Delimiter $$
drop procedure if exists sp_select_autor $$
create procedure sp_select_autor(id_aut int)
begin
	select id, nome from Autores where id = id_aut;
end $$

Delimiter $$
Drop procedure if exists sp_editar_autor $$
create procedure sp_editar_autor(id_aut int, nome_aut varchar(150))
Begin

	Update Autores
    set nome = nome_aut
	where id = id_aut;
    
end $$

Delimiter $$
Drop procedure if exists sp_deletar_autor $$
create procedure sp_deletar_autor(id_aut int)
begin

	delete from Autores where id = id_aut;

end $$

-- Genero!!!!

Delimiter $$
drop procedure if exists sp_select_genero $$
create procedure sp_select_genero(id_gen int)
begin
	select id, nome from Generos where id = id_gen;
end $$

Delimiter $$
Drop procedure if exists sp_editar_genero $$
create procedure sp_editar_genero(id_gen int, nome_gen varchar(150))
Begin

	Update Generos
    set nome = nome_gen
	where id = id_gen;
    
end $$

Delimiter $$
Drop procedure if exists sp_deletar_genero $$
create procedure sp_deletar_genero(id_gen int)
begin

	delete from Generos where id = id_gen;

end $$


Delimiter $$
Drop procedure if exists sp_vitrine_buscar $$
create procedure sp_vitrine_buscar(In p_q varchar(200))
begin
	Select 
    l.id, l.titulo, l.autorId, l.editoraId, l.generoId, l.ano, l.isbn, l.capa_arquivo, 
    l.quantidade_total, l.quantidade_disponivel
    from Livros l 
    where l.quantidade_disponivel > 0
		and (p_q is null or p_q = '' or l.titulo like concat('%', p_q, '%'))
	Order by l.titulo;
end $$


Delimiter $$
drop procedure if exists sp_livro_listar_por_ids $$
create procedure sp_livro_listar_por_ids(in p_ids TEXT)
begin
	/* p_ids : string CSV, ex.: '1,5,9'*/
	
	select l.id, l.titulo, l.capa_arquivo, l.quantidade_disponivel
    from Livros l
    where FIND_IN_SET(l.id, p_ids) > 0
    order by l.titulo;

end $$


Delimiter $$
drop procedure if exists sp_listar_leitor_list $$
create procedure sp_listar_leitor_list()
begin	
	select id_leitor, nomeLeitor
    from Livros l
    order by nomeLeitor;
end $$


Delimiter $$
drop procedure if exists sp_emprestimo_adicionar_item $$
create procedure sp_emprestimo_adicionar_item (
in p_id_emprestimo int,
in p_id_livro int,
in p_qtd int
)
begin
declare v_disp int;

if p_qtd is null or p_qtd <= 0 then
		signal SQLSTATE '45000' SET MESSAGE_TEXT = 'Quantidade inválida';
end if;

select quantidade_disponivel into v_disp from Livros where id = p_id_livro for Update;

if v_disp is null then
	signal SQLSTATE '45000' SET MESSAGE_TEXT = 'Livro inexistente';
end if;

if v_disp < p_qtd then
	signal SQLSTATE '45000' SET MESSAGE_TEXT = 'Estoque insuficiente para este livro';
end if;

	insert into emprestimo_itens(id_emprestimo, id_livro, quantidade)
						values(p_id_emprestimo, p_id_livro, p_qtd);
	
    update Livros
		set  quantidade_disponivel = quantidade_disponivel - p_qtd
        where id = p_id_livro;

end $$

describe Livros;

Delimiter $$
drop procedure if exists sp_emprestimo_criar $$
create procedure sp_emprestimo_criar (
in p_id_leitor int,
in p_id_bibliotecario int,
in p_data_prevista DATE,
out p_id_gerado int
)
begin

	Insert into emprestimos(id_leitor, id_bibliotecario, data_prevista_devolucao)
					values(p_id_leitor, p_id_bibliotecario, p_data_prevista);
                    set p_id_gerado = Last_Insert_Id();

end $$




describe emprestimos;
describe leitor;





describe Livros;




describe Editoras;
select * from Livros;
select * from Usuarios;
select * from Generos;
Select * from Editoras;
select * from Autores;