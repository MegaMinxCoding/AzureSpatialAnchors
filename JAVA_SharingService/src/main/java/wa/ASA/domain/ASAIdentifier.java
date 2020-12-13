package wa.ASA.domain;

import java.sql.Timestamp;

import javax.persistence.Entity;
import javax.persistence.Id;

@Entity
public class ASAIdentifier {

	@Id
	String id;

	Timestamp timeStamp;

	public ASAIdentifier() {

	}

	public ASAIdentifier(String id) {
		this.id = id;
		timeStamp = new Timestamp(System.currentTimeMillis());
	}

	public void setTimeStamp() {
		timeStamp = new Timestamp(System.currentTimeMillis());
	}

	public String getId() {
		return id;
	}

	public Timestamp getTimeStamp() {
		return timeStamp;
	}

}
